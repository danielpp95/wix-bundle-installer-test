namespace InstallerUI.Pages
{
    using System.Windows;
    using System.Windows.Forms;

    public partial class InstallPathPage : BasePage
    {
        public InstallPathPage()
        {
            this.InitializeComponent();
            this.PageTitle = Strings.InstallPathTitle;
            this.Buttons = new[]
            {
                new DialogButtonSpec(Strings.CancelButton, true, (s, e) => this.Host.Cancel(), isCancel: true),
                new DialogButtonSpec(Strings.PrevButton, true, (s, e) => this.Host.ShowPreviousPage()),
                new DialogButtonSpec(Strings.NextButton, true, (s, e) => this.Host.ShowNextPage(), isDefault: true),
            };
        }

        public string InstallPath
        {
            get => this.PathTextBox.Text;
            set => this.PathTextBox.Text = value;
        }

        private void OnBrowseClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = this.InstallPath;
                dialog.Description = "Select the folder where the application will be installed.";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.InstallPath = dialog.SelectedPath;
                }
            }
        }
    }
}
