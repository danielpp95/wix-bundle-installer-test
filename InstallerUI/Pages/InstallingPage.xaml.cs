namespace InstallerUI.Pages
{
    using System;
    using System.Collections.ObjectModel;
    using InstallerUI.Models;

    public partial class InstallingPage : BasePage
    {
        public InstallingPage(ObservableCollection<PackageProgressItem> packages, InstallerContext context)
        {
            this.InitializeComponent();
            this.PackagesList.ItemsSource = packages;
            this.Loader.StatusText = Strings.ProceedingStatus;

            switch (context.Flow)
            {
                case InstallerFlow.Uninstall:
                    this.PageTitle = Strings.UninstallingTitle;
                    this.StepLabels = Strings.UninstallSteps;
                    this.StepIndex = 0;
                    break;
                case InstallerFlow.Repair:
                    this.PageTitle = Strings.RepairingTitle;
                    this.StepLabels = Strings.RepairSteps;
                    this.StepIndex = 0;
                    break;
                default:
                    this.PageTitle = Strings.InstallingTitle;
                    this.StepLabels = Strings.InstallSteps;
                    this.StepIndex = 3;
                    break;
            }

            // Cancel is the only live control here; Prev/Next are shown disabled purely so
            // the button bar doesn't visibly shrink and grow between pages.
            this.Buttons = new[]
            {
                new DialogButtonSpec(Strings.CancelButton, true, (s, e) => this.Host.Cancel(), isCancel: true),
                new DialogButtonSpec(Strings.PrevButton, false, (s, e) => { }),
                new DialogButtonSpec(Strings.NextButton, false, (s, e) => { }),
            };
        }

        public int OverallPercentage
        {
            get => this.Loader.Percentage;
            set => this.Loader.Percentage = value;
        }

        public void AppendLog(string line)
        {
            this.LogTextBox.AppendText(line + Environment.NewLine);
            this.LogTextBox.ScrollToEnd();
        }
    }
}
