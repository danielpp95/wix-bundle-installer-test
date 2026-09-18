namespace InstallerUI.Pages
{
    using System.Windows;
    using System.Windows.Forms;
    using UserControl = System.Windows.Controls.UserControl;

    public partial class InstallPathPage : UserControl
    {
        public InstallPathPage()
        {
            this.InitializeComponent();
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
