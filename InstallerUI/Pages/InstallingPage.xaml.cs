namespace InstallerUI.Pages
{
    using System.Collections.ObjectModel;
    using System.Windows.Controls;
    using InstallerUI.Models;

    public partial class InstallingPage : BasePage
    {
        public InstallingPage(ObservableCollection<PackageProgressItem> packages)
        {
            this.InitializeComponent();
            this.PackagesList.ItemsSource = packages;
            this.PageTitle = InstallerBootstrapperApplication.IsUninstall ? "Uninstalling" : "Installing";
            this.buttons = new[]
            {
                new DialogButtonSpec("Cancel", true, (s, e) => this.Cancel(), isCancel: true),
                new DialogButtonSpec("Prev", false, (s, e) => { }),
                new DialogButtonSpec("Next", false, (s, e) => { }),
            };
        }

        public int OverallPercentage
        {
            get => (int)this.OverallProgressBar.Value;
            set => this.OverallProgressBar.Value = value;
        }

        public void AppendLog(string line)
        {
            this.LogTextBox.AppendText(line + System.Environment.NewLine);
            this.LogTextBox.ScrollToEnd();
        }
    }
}
