namespace InstallerUI
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows;
    using System.Windows.Interop;
    using InstallerUI.Models;
    using InstallerUI.Pages;
    using WixToolset.BootstrapperApplicationApi;
    using ErrorEventArgs = WixToolset.BootstrapperApplicationApi.ErrorEventArgs;

    /// <summary>
    /// Custom out-of-process bootstrapper application (WiX v5 Burn model).
    ///
    /// Burn can launch this with four different Display levels (WixBundleUILevel /
    /// IBootstrapperCommand.Display): Full (a person is watching and clicking through the
    /// wizard - e.g. someone double-clicked the exe), Passive (show progress, no
    /// interaction - what msiexec /passive does), and None/Embedded (fully silent - what
    /// SCCM/Intune/CI actually use in practice). Only Full builds the multi-page wizard;
    /// Passive shows just the installing page and closes itself; None/Embedded never
    /// touches WPF at all. Detect/Plan/Apply/Execute callback handling is shared across
    /// all three so there is exactly one place that knows what a given engine event means.
    /// </summary>
    public class InstallerBootstrapperApplication : BootstrapperApplication, INavigationHost
    {
        private const string InstallFolderVariable = "InstallFolder";
        private const string DefaultInstallFolder = @"C:\Program Files\MyApp";

        private readonly ObservableCollection<PackageProgressItem> _packages = new ObservableCollection<PackageProgressItem>();
        private readonly Dictionary<string, PackageProgressItem> _packagesById = new Dictionary<string, PackageProgressItem>();

        private Application _app;
        private MainWindow _window;
        private IBootstrapperCommand _command;
        private InstallerContext _context;

        // True for Passive/None/Embedded: nobody is present to click Next/Install/Finish,
        // so Detect success should walk straight into Plan/Apply, and Apply completion
        // should close things down on its own instead of showing an Installed/Error page.
        private bool _unattended;
        private IntPtr _hwnd = IntPtr.Zero;

        private List<BasePage> installationPages;
        private List<BasePage> maintenancePages;
        private int actualPageIndex;

        private PresentationPage _presentationPage;
        private EulaPage _eulaPage;
        private InstallPathPage _installPathPage;
        private ReadyToInstallPage _readyToInstallPage;
        private InstallingPage _installingPage;
        private InstalledPage _installedPage;
        private ErrorPage _errorPage;

        private bool _isApplying;
        private bool _userCanceled;
        private bool _applyCompleted;
        private bool _rollbackStarted;

        // Exit code contract: 0 = success, 1223 (ERROR_CANCELLED) = the user backed out
        // before or during Apply, anything else is the raw HRESULT Burn reported so it can
        // be looked up like any other Windows Installer failure code.
        private int _finalExitCode;

        public InstallerBootstrapperApplication()
        {
            this.DetectComplete += this.OnDetectCompleteHandler;
            this.PlanComplete += this.OnPlanCompleteHandler;
            this.ApplyComplete += this.OnApplyCompleteHandler;
            this.ExecutePackageBegin += this.OnExecutePackageBeginHandler;
            this.ExecutePackageComplete += this.OnExecutePackageCompleteHandler;
            this.ExecuteProgress += this.OnExecuteProgressHandler;
            this.CacheAcquireProgress += this.OnCacheAcquireProgressHandler;
            this.CacheVerifyProgress += this.OnCacheVerifyProgressHandler;
            this.CacheAcquireResolving += this.OnCacheAcquireResolvingHandler;
            this.Error += this.OnErrorHandler;
        }

        protected override void OnCreate(CreateEventArgs args)
        {
            base.OnCreate(args);

            this._command = args.Command;
            this._context = new InstallerContext(this._command.Action);
        }

        protected override void Run()
        {
            try
            {
                this.LoadPackageList(this._command.BootstrapperApplicationDataPath);

                var display = this._command.Display;
                var showWizard = display == Display.Full || display == Display.Unknown;
                var showWindow = showWizard || display == Display.Passive;

                this.RunWithWindow(showWizard, showWindow);
            }
            catch (Exception ex)
            {
                // A bug in the UI must never leave a package half-applied with no trace of
                // why - always get something into the Burn log before we go down.
                this.engine.Log(LogLevel.Error, "Unhandled exception in bootstrapper application: " + ex);
                this._finalExitCode = this._finalExitCode == 0 ? unchecked((int)0x80004005) : this._finalExitCode; // E_FAIL fallback
            }

            this.engine.Quit(this._finalExitCode);
        }

        /// <summary>
        /// Every Display level pumps the same WPF message loop and reports through the same
        /// engine-callback handlers below - Burn's Apply() rejects a null window handle even
        /// for Display.None/Embedded, so there is always a real window, it just stays
        /// off-screen and pageless for the two unattended levels (see
        /// MainWindow.HideForUnattendedRun).
        /// </summary>
        private void RunWithWindow(bool showWizard, bool showWindow)
        {
            this._unattended = !showWizard;

            this._app = new Application();
            LoadStyles(this._app);

            this._window = new MainWindow();
            this._window.Closing += (s, e) =>
            {
                if (!this._applyCompleted)
                {
                    this._finalExitCode = 1223; // ERROR_CANCELLED
                }
            };

            if (!showWindow)
            {
                this._window.HideForUnattendedRun();
            }
            else
            {
                this.CreatePages();

                if (showWizard)
                {
                    this.ShowPageAt(0);
                }
                else
                {
                    // Passive: no wizard, just live progress; Detect success drives
                    // Plan/Apply automatically and the window closes itself when done.
                    this._window.Navigate(this._installingPage);
                }
            }

            // Detect/Plan callbacks arrive on Burn's own callback thread (not the WPF
            // Dispatcher thread), so they can race ahead of _app.Run() getting around to
            // showing the window. EnsureHandle() forces the HWND to exist synchronously,
            // right now, so Apply(_hwnd) below is never handed a null handle - which Burn
            // rejects outright, even for a fully silent install.
            this._hwnd = new WindowInteropHelper(this._window).EnsureHandle();

            this.engine.Detect();

            this._app.Run(this._window);
        }

        /// <summary>
        /// Merges the shared style dictionaries into Application.Resources once, so every
        /// page's {StaticResource}/{DynamicResource} lookups resolve regardless of which
        /// page loads first. There is no App.xaml to do this declaratively (see Program.cs).
        /// </summary>
        private static void LoadStyles(Application app)
        {
            foreach (var name in new[] { "Colors", "Typography", "Buttons", "CheckBox" })
            {
                var uri = new Uri($"pack://application:,,,/InstallerUI;component/Styles/{name}.xaml", UriKind.Absolute);
                app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
            }
        }

        private void CreatePages()
        {
            this._presentationPage = new PresentationPage { Host = this };
            this._eulaPage = new EulaPage { Host = this };
            this._installPathPage = new InstallPathPage { Host = this, InstallPath = this.GetDefaultInstallFolder() };
            this._readyToInstallPage = new ReadyToInstallPage(this._context) { Host = this };
            this._installingPage = new InstallingPage(this._packages, this._context) { Host = this };
            this._installedPage = new InstalledPage(this._context) { Host = this };
            this._errorPage = new ErrorPage(this._context) { Host = this };

            this.installationPages = new List<BasePage>
            {
                this._presentationPage,
                this._eulaPage,
                this._installPathPage,
                this._readyToInstallPage,
                this._installingPage,
                this._installedPage,
            };

            // Uninstall and repair/modify both skip straight to the same three pages; each
            // page already adapts its own wording from the InstallerContext it was built with.
            this.maintenancePages = new List<BasePage>
            {
                this._readyToInstallPage,
                this._installingPage,
                this._installedPage,
            };
        }

        private List<BasePage> ActivePages => this._context.IsMaintenance ? this.maintenancePages : this.installationPages;

        // ----- INavigationHost (what a page is allowed to ask the shell to do) -----

        void INavigationHost.ShowNextPage() => this.ShowPageAt(this.actualPageIndex + 1);

        void INavigationHost.ShowPreviousPage() => this.ShowPageAt(this.actualPageIndex - 1);

        void INavigationHost.Cancel() => this.OnCancelClicked();

        void INavigationHost.Finish() => this._window.Close();

        void INavigationHost.RefreshButtons(BasePage page) => this._window.Navigate(page);

        private void ShowPageAt(int index)
        {
            var pages = this.ActivePages;
            if (index < 0 || index >= pages.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            this.actualPageIndex = index;

            var page = pages[index];
            this._window.Navigate(page);

            // Arriving at the installing page is what actually kicks off the engine; it
            // isn't a page the user can navigate to any other way.
            if (ReferenceEquals(page, this._installingPage) && !this._isApplying)
            {
                this.BeginApply();
            }
        }

        private void BeginApply()
        {
            this._isApplying = true;

            // _installPathPage only exists in the full wizard (Display.Full/Unknown) - for
            // Passive/None/Embedded there's no page to confirm a path from, so the bundle's
            // authored default (see the InstallFolder Variable in Bundle.wxs) stands as-is.
            if (this._context.Flow == InstallerFlow.Install && this._installPathPage != null)
            {
                try
                {
                    this.engine.SetVariableString(InstallFolderVariable, this._installPathPage.InstallPath, false);
                }
                catch (Exception ex)
                {
                    this.engine.Log(LogLevel.Error, "Failed to set " + InstallFolderVariable + ": " + ex.Message);
                }
            }

            this.engine.Plan(this._context.Action);
        }

        private void OnCancelClicked()
        {
            this._userCanceled = true;

            if (!this._isApplying)
            {
                this._window.Close();
            }
        }

        // ----- Shared failure/completion plumbing for all three Display levels -----

        private void HandleEarlyFailure(string message, int status)
        {
            this._finalExitCode = status;
            this.engine.Log(LogLevel.Error, message + " " + FormatHresult(status));

            if (this._unattended)
            {
                this.CompleteUnattended();
            }
            else
            {
                this._window.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this._errorPage.Message = message;
                    this._errorPage.Details = FormatHresult(status);
                    this._window.Navigate(this._errorPage);
                }));
            }
        }

        private void CompleteUnattended() =>
            this._window.Dispatcher.BeginInvoke(new Action(() => this._window.Close()));

        private string GetDefaultInstallFolder()
        {
            try
            {
                return this.engine.FormatString(this.engine.GetVariableString(InstallFolderVariable));
            }
            catch (Exception ex)
            {
                this.engine.Log(LogLevel.Error, $"Could not read the '{InstallFolderVariable}' bundle variable, falling back to a default install path: {ex.Message}");
                return DefaultInstallFolder;
            }
        }

        private void LoadPackageList(string bootstrapperApplicationDataPath)
        {
            try
            {
                var data = new BootstrapperApplicationData(new FileInfo(bootstrapperApplicationDataPath));
                foreach (var package in data.Bundle.Packages.Values)
                {
                    var displayName = string.IsNullOrEmpty(package.DisplayName) ? package.Id : package.DisplayName;
                    var item = new PackageProgressItem(package.Id, displayName);
                    this._packages.Add(item);
                    this._packagesById[package.Id] = item;
                }
            }
            catch (Exception ex)
            {
                this.engine.Log(LogLevel.Error, "Failed to read bundle package list: " + ex.Message);
            }
        }

        // ----- Engine callbacks -----
        //
        // These arrive on Burn's native callback thread (through mbanative.dll), not the UI
        // thread, so anything touching WPF objects is marshaled through the Dispatcher. Each
        // handler is wrapped in try/catch: an exception crossing back into native code
        // mid-transaction is undefined behavior on the engine side, so a UI bug must never
        // be allowed to propagate out of here - log it and let the install/uninstall continue
        // (or fail cleanly through the normal Complete event) rather than corrupt engine state.

        private void OnDetectCompleteHandler(object sender, DetectCompleteEventArgs e)
        {
            try
            {
                if (e.Status < 0)
                {
                    this.HandleEarlyFailure(Strings.DetectFailedMessage, e.Status);
                }
                else if (this._unattended)
                {
                    this.BeginApply();
                }
            }
            catch (Exception ex)
            {
                this.engine.Log(LogLevel.Error, "Unhandled exception in " + nameof(this.OnDetectCompleteHandler) + ": " + ex);
            }
        }

        private void OnPlanCompleteHandler(object sender, PlanCompleteEventArgs e)
        {
            try
            {
                if (e.Status >= 0)
                {
                    this.engine.Apply(this._hwnd);
                }
                else
                {
                    this.HandleEarlyFailure(Strings.PlanFailedMessage, e.Status);
                }
            }
            catch (Exception ex)
            {
                this.engine.Log(LogLevel.Error, "Unhandled exception in " + nameof(this.OnPlanCompleteHandler) + ": " + ex);
            }
        }

        private void OnApplyCompleteHandler(object sender, ApplyCompleteEventArgs e)
        {
            try
            {
                this._applyCompleted = true;
                this._isApplying = false;
                this._finalExitCode = e.Status >= 0 ? 0 : e.Status;

                if (e.Status < 0)
                {
                    var reason = this._userCanceled ? Strings.CanceledMessage : this.GetFailureMessage();
                    this.engine.Log(LogLevel.Error, reason + " " + FormatHresult(e.Status));
                }

                if (this._unattended)
                {
                    this.CompleteUnattended();
                }
                else
                {
                    this._window.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (e.Status >= 0)
                        {
                            ((INavigationHost)this).ShowNextPage(); // -> InstalledPage
                        }
                        else
                        {
                            var reason = this._userCanceled ? Strings.CanceledMessage : this.GetFailureMessage();
                            this._errorPage.Message = reason;
                            this._errorPage.Details = FormatHresult(e.Status);
                            this._window.Navigate(this._errorPage);
                        }
                    }));
                }

                _ = e.Restart; // reboot prompting can be layered on here later if a package ever requires it.
            }
            catch (Exception ex)
            {
                this.engine.Log(LogLevel.Error, "Unhandled exception in " + nameof(this.OnApplyCompleteHandler) + ": " + ex);
            }
        }

        private string GetFailureMessage()
        {
            switch (this._context.Flow)
            {
                case InstallerFlow.Uninstall:
                    return Strings.UninstallFailedMessage;
                case InstallerFlow.Repair:
                    return Strings.RepairFailedMessage;
                default:
                    return Strings.InstallFailedMessage;
            }
        }

        private void OnExecutePackageBeginHandler(object sender, ExecutePackageBeginEventArgs e)
        {
            try
            {
                if (this._userCanceled)
                {
                    e.Cancel = true;
                }

                if (string.IsNullOrEmpty(e.PackageId) || !this._packagesById.TryGetValue(e.PackageId, out var item))
                {
                    return;
                }

                var rollingBack = this._rollbackStarted;

                this.RunOnUiThreadIfPresent(() =>
                {
                    item.Status = rollingBack ? PackageStepStatus.RollingBack : PackageStepStatus.Installing;
                    this._installingPage?.AppendLog((rollingBack ? "Rolling back " : "Installing ") + item.DisplayName + "...");
                });
            }
            catch (Exception ex)
            {
                this.engine.Log(LogLevel.Error, "Unhandled exception in " + nameof(this.OnExecutePackageBeginHandler) + ": " + ex);
            }
        }

        private void OnExecutePackageCompleteHandler(object sender, ExecutePackageCompleteEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.PackageId) || !this._packagesById.TryGetValue(e.PackageId, out var item))
                {
                    return;
                }

                var succeeded = e.Status >= 0;
                if (!succeeded)
                {
                    // Burn doesn't hand us an explicit "this execution is a rollback" flag on
                    // these events; a package failing is what triggers rollback of whatever
                    // already succeeded, so anything executed after this point is a rollback
                    // step. This is a heuristic, not a guarantee from the API.
                    this._rollbackStarted = true;
                }

                var wasRollingBack = item.Status == PackageStepStatus.RollingBack;

                this.RunOnUiThreadIfPresent(() =>
                {
                    item.Status = succeeded ? PackageStepStatus.Done : PackageStepStatus.Failed;
                    var verb = wasRollingBack ? "Rolled back" : "Done";
                    this._installingPage?.AppendLog($"{item.DisplayName}: {(succeeded ? verb : FormatHresult(e.Status))}");
                });
            }
            catch (Exception ex)
            {
                this.engine.Log(LogLevel.Error, "Unhandled exception in " + nameof(this.OnExecutePackageCompleteHandler) + ": " + ex);
            }
        }

        private void OnExecuteProgressHandler(object sender, ExecuteProgressEventArgs e)
        {
            try
            {
                if (this._userCanceled)
                {
                    e.Cancel = true;
                }

                this.UpdateOverallProgress(e.OverallPercentage);
            }
            catch (Exception ex)
            {
                this.engine.Log(LogLevel.Error, "Unhandled exception in " + nameof(this.OnExecuteProgressHandler) + ": " + ex);
            }
        }

        private void OnCacheAcquireProgressHandler(object sender, CacheAcquireProgressEventArgs e)
        {
            try
            {
                if (this._userCanceled)
                {
                    e.Cancel = true;
                }

                this.UpdateOverallProgress(e.OverallPercentage);
            }
            catch (Exception ex)
            {
                this.engine.Log(LogLevel.Error, "Unhandled exception in " + nameof(this.OnCacheAcquireProgressHandler) + ": " + ex);
            }
        }

        private void OnCacheVerifyProgressHandler(object sender, CacheVerifyProgressEventArgs e)
        {
            try
            {
                if (this._userCanceled)
                {
                    e.Cancel = true;
                }

                this.UpdateOverallProgress(e.OverallPercentage);
            }
            catch (Exception ex)
            {
                this.engine.Log(LogLevel.Error, "Unhandled exception in " + nameof(this.OnCacheVerifyProgressHandler) + ": " + ex);
            }
        }

        private void OnCacheAcquireResolvingHandler(object sender, CacheAcquireResolvingEventArgs e)
        {
            try
            {
                // No percentage to report here, but honoring cancel this early means a
                // download that hasn't started yet doesn't have to start at all.
                if (this._userCanceled)
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                this.engine.Log(LogLevel.Error, "Unhandled exception in " + nameof(this.OnCacheAcquireResolvingHandler) + ": " + ex);
            }
        }

        private void OnErrorHandler(object sender, ErrorEventArgs e)
        {
            try
            {
                this.engine.Log(LogLevel.Error, e.ErrorMessage);

                this.RunOnUiThreadIfPresent(() => this._installingPage?.AppendLog("Error: " + e.ErrorMessage));
            }
            catch (Exception ex)
            {
                this.engine.Log(LogLevel.Error, "Unhandled exception in " + nameof(this.OnErrorHandler) + ": " + ex);
            }
        }

        private void UpdateOverallProgress(int overallPercentage) =>
            this.RunOnUiThreadIfPresent(() =>
            {
                if (this._installingPage != null)
                {
                    this._installingPage.OverallPercentage = overallPercentage;
                }
            });

        private void RunOnUiThreadIfPresent(Action action)
        {
            if (this._window == null)
            {
                return;
            }

            // BeginInvoke's callback runs later, on the dispatcher thread, entirely outside
            // the try/catch of whichever On*Handler queued it - an exception thrown in here
            // does NOT get caught by that handler's own try/catch. It has to be handled here
            // instead, or it surfaces as an "unhandled exception" several frames removed from
            // the mistake that actually caused it.
            this._window.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    this.engine.Log(LogLevel.Error, "Unhandled exception on the UI thread: " + ex);
                }
            }));
        }

        private static string FormatHresult(int hresult) => $"HRESULT: 0x{hresult:X8}";
    }
}
