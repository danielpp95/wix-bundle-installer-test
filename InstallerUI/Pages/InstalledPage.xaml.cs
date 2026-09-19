namespace InstallerUI.Pages
{
    using System.Windows.Controls;

    public partial class InstalledPage : BasePage
    {
        public InstalledPage()
        {
            this.InitializeComponent();
            this.PageTitle = InstallerBootstrapperApplication.IsUninstall ? "Uninstalled" : "Installed successfully";
            this.buttons = new[]
            {
                new DialogButtonSpec("Finish", true, (s, e) => this.Finish(), isDefault: true, isCancel: true),
            };
        }

        public string Message
        {
            get => this.MessageText.Text;
            set => this.MessageText.Text = value;
        }

        public bool ShowLaunchOption
        {
            get => this.LaunchAppCheckBox.Visibility == System.Windows.Visibility.Visible;
            set => this.LaunchAppCheckBox.Visibility = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public bool LaunchAppRequested => this.LaunchAppCheckBox.IsChecked == true;
    }
}
