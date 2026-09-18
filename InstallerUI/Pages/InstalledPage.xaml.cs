namespace InstallerUI.Pages
{
    using System.Windows.Controls;

    public partial class InstalledPage : UserControl
    {
        public InstalledPage()
        {
            this.InitializeComponent();
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
