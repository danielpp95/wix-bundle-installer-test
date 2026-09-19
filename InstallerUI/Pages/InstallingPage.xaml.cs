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

            this.PageTitle = context.Flow switch
            {
                InstallerFlow.Uninstall => Strings.UninstallingTitle,
                InstallerFlow.Repair => Strings.RepairingTitle,
                _ => Strings.InstallingTitle,
            };

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
            get => (int)this.OverallProgressBar.Value;
            set => this.OverallProgressBar.Value = value;
        }

        public void AppendLog(string line)
        {
            this.LogTextBox.AppendText(line + Environment.NewLine);
            this.LogTextBox.ScrollToEnd();
        }
    }
}
