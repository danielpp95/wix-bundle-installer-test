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
    /// Owns the WPF shell and drives it from the Burn engine callbacks.
    /// </summary>
    public class InstallerBootstrapperApplication : BootstrapperApplication
    {
        private const string InstallFolderVariable = "InstallFolder";

        private readonly ObservableCollection<PackageProgressItem> _packages = new ObservableCollection<PackageProgressItem>();
        private readonly Dictionary<string, PackageProgressItem> _packagesById = new Dictionary<string, PackageProgressItem>();

        private Application _app;
        private MainWindow _window;
        private IBootstrapperCommand _command;

        private List<BasePage> installationPages;
        private List<BasePage> uninstallPages;
        private int actualPageIndex;

        private PresentationPage _presentationPage;
        private EulaPage _eulaPage;
        private InstallPathPage _installPathPage;
        private ReadyToInstallPage _readyToInstallPage;
        private InstallingPage _installingPage;
        private InstalledPage _installedPage;
        private ErrorPage _errorPage;

        private static bool _isUninstall;
        public static bool IsUninstall => _isUninstall;
        private bool _isApplying;
        private bool _userCanceled;
        private bool _applyCompleted;
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
            this.Error += this.OnErrorHandler;
        }

        protected override void OnCreate(CreateEventArgs args)
        {
            base.OnCreate(args);

            this._command = args.Command;
            _isUninstall = this._command.Action == LaunchAction.Uninstall;

            this.LoadPackageList(this._command.BootstrapperApplicationDataPath);
        }

        protected override void Run()
        {
            this._app = new Application();

            this._window = new MainWindow();
            this._window.Closing += (s, e) =>
            {
                if (!this._applyCompleted)
                {
                    this._finalExitCode = 1223; // ERROR_CANCELLED
                }
            };

            this._presentationPage = new PresentationPage();
            this._eulaPage = new EulaPage();
            this._eulaPage.AcceptedChanged += (s, e) => this._window.Navigate(this._eulaPage);
            this._installPathPage = new InstallPathPage { InstallPath = this.GetDefaultInstallFolder() };
            this._readyToInstallPage = new ReadyToInstallPage();
            this._installingPage = new InstallingPage(this._packages);
            this._installedPage = new InstalledPage();
            this._errorPage = new ErrorPage();

            this.installationPages = new List<BasePage>
            {
                this._presentationPage,
                this._eulaPage,
                this._installPathPage,
                this._readyToInstallPage,
                this._installingPage,
                this._installedPage,
            };

            this.uninstallPages = new List<BasePage>
            {
                this._readyToInstallPage,
                this._installingPage,
                this._installedPage,
            };

            foreach (var page in this.installationPages)
            {
                page.ShowNextPage = this.ShowNextPage;
                page.ShowPreviousPage = this.ShowPreviousPage;
                page.Cancel = this.OnCancelClicked;
                page.Finish = this.OnFinishClicked;
            }

            foreach (var page in this.uninstallPages)
            {
                page.ShowNextPage = this.ShowNextPage;
                page.ShowPreviousPage = this.ShowPreviousPage;
                page.Cancel = this.OnCancelClicked;
                page.Finish = this.OnFinishClicked;
            }

            this.ShowPageAt(0);

            this.engine.Detect();

            this._app.Run(this._window);

            this.engine.Quit(this._finalExitCode);
        }

        // ----- Navigation -----

        private void ShowPageAt(int index)
        {
            var pages = _isUninstall ? this.uninstallPages : this.installationPages;
            if (index < 0 || index >= pages.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            this.actualPageIndex = index;

            var page = pages[index];
            this._window.Navigate(page);

            // Arriving at the installing page is what actually kicks off the engine;
            // it isn't a page the user can navigate to any other way.
            if (ReferenceEquals(page, this._installingPage) && !this._isApplying)
            {
                this.BeginApply();
            }
        }

        private void ShowNextPage() => this.ShowPageAt(this.actualPageIndex + 1);

        private void ShowPreviousPage() => this.ShowPageAt(this.actualPageIndex - 1);

        private void BeginApply()
        {
            this._isApplying = true;

            if (!_isUninstall)
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

            this.engine.Plan(_isUninstall ? LaunchAction.Uninstall : LaunchAction.Install);
        }

        private void ShowErrorPage(string message, string details)
        {
            this._isApplying = false;

            this._errorPage.PageTitle = _isUninstall ? "Uninstall failed" : "Setup failed";
            this._errorPage.Message = message;
            this._errorPage.Details = details;

            this._window.Navigate(this._errorPage);
        }

        private void OnCancelClicked()
        {
            this._userCanceled = true;

            if (!this._isApplying)
            {
                this._window.Close();
            }
        }

        private void OnFinishClicked() => this._window.Close();

        private string GetDefaultInstallFolder()
        {
            try
            {
                return this.engine.FormatString(this.engine.GetVariableString(InstallFolderVariable));
            }
            catch (Exception)
            {
                return string.Empty;
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

        // ----- Engine callbacks (arrive on a non-UI thread; marshal to the dispatcher) -----

        private void OnDetectCompleteHandler(object sender, DetectCompleteEventArgs e)
        {
            if (e.Status < 0)
            {
                this._window.Dispatcher.BeginInvoke(new Action(() =>
                    this.ShowErrorPage("Detecting the current state of the machine failed.", FormatHresult(e.Status))));
            }
        }

        private void OnPlanCompleteHandler(object sender, PlanCompleteEventArgs e)
        {
            if (e.Status >= 0)
            {
                var hwnd = IntPtr.Zero;
                this._window.Dispatcher.Invoke(new Action(() => hwnd = new WindowInteropHelper(this._window).EnsureHandle()));
                this.engine.Apply(hwnd);
            }
            else
            {
                this._window.Dispatcher.BeginInvoke(new Action(() =>
                    this.ShowErrorPage("Planning the installation failed.", FormatHresult(e.Status))));
            }
        }

        private void OnApplyCompleteHandler(object sender, ApplyCompleteEventArgs e)
        {
            this._applyCompleted = true;
            this._finalExitCode = e.Status >= 0 ? 0 : e.Status;

            this._window.Dispatcher.BeginInvoke(new Action(() =>
            {
                this._isApplying = false;

                if (e.Status >= 0)
                {
                    this._installedPage.Message = _isUninstall
                        ? "The app was uninstalled successfully"
                        : "The app was installed successfully";
                    this._installedPage.ShowLaunchOption = !_isUninstall;
                    this.ShowNextPage();
                }
                else if (this._userCanceled)
                {
                    this.ShowErrorPage("The installation was canceled.", FormatHresult(e.Status));
                }
                else
                {
                    this.ShowErrorPage(_isUninstall ? "The uninstall failed." : "The installation failed.", FormatHresult(e.Status));
                }
            }));

            _ = e.Restart; // reboot prompting can be layered on here later if a package ever requires it.
        }

        private void OnExecutePackageBeginHandler(object sender, ExecutePackageBeginEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PackageId) || !this._packagesById.TryGetValue(e.PackageId, out var item))
            {
                return;
            }

            this._window.Dispatcher.BeginInvoke(new Action(() =>
            {
                item.Status = PackageStepStatus.Installing;
                this._installingPage.AppendLog($"Installing {item.DisplayName}...");
            }));
        }

        private void OnExecutePackageCompleteHandler(object sender, ExecutePackageCompleteEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PackageId) || !this._packagesById.TryGetValue(e.PackageId, out var item))
            {
                return;
            }

            var succeeded = e.Status >= 0;

            this._window.Dispatcher.BeginInvoke(new Action(() =>
            {
                item.Status = succeeded ? PackageStepStatus.Done : PackageStepStatus.Failed;
                this._installingPage.AppendLog($"{item.DisplayName}: {(succeeded ? "Done" : FormatHresult(e.Status))}");
            }));
        }

        private void OnExecuteProgressHandler(object sender, ExecuteProgressEventArgs e)
        {
            if (this._userCanceled)
            {
                e.Cancel = true;
            }

            this._window.Dispatcher.BeginInvoke(new Action(() => this._installingPage.OverallPercentage = e.OverallPercentage));
        }

        private void OnCacheAcquireProgressHandler(object sender, CacheAcquireProgressEventArgs e)
        {
            if (this._userCanceled)
            {
                e.Cancel = true;
            }

            this._window.Dispatcher.BeginInvoke(new Action(() => this._installingPage.OverallPercentage = e.OverallPercentage));
        }

        private void OnErrorHandler(object sender, ErrorEventArgs e)
        {
            this.engine.Log(LogLevel.Error, e.ErrorMessage);

            this._window.Dispatcher.BeginInvoke(new Action(() => this._installingPage.AppendLog("Error: " + e.ErrorMessage)));
        }

        private static string FormatHresult(int hresult) => $"HRESULT: 0x{hresult:X8}";
    }
}
