namespace InstallerUI.Pages
{
    using System.Collections.ObjectModel;
    using System.Windows.Controls;
    using InstallerUI.Models;

    public partial class InstallingPage : UserControl
    {
        public InstallingPage(ObservableCollection<PackageProgressItem> packages)
        {
            this.InitializeComponent();
            this.PackagesList.ItemsSource = packages;
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
