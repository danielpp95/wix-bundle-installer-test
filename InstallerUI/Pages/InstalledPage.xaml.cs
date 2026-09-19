namespace InstallerUI.Pages
{
    using InstallerUI.Models;

    public partial class InstalledPage : BasePage
    {
        public InstalledPage(InstallerContext context)
        {
            this.InitializeComponent();

            switch (context.Flow)
            {
                case InstallerFlow.Uninstall:
                    this.PageTitle = Strings.UninstalledTitle;
                    this.Message = Strings.UninstalledMessage;
                    break;
                case InstallerFlow.Repair:
                    this.PageTitle = Strings.RepairedTitle;
                    this.Message = Strings.RepairedMessage;
                    break;
                default:
                    this.PageTitle = Strings.InstalledTitle;
                    this.Message = Strings.InstalledMessage;
                    break;
            }

            // "Open the app on finish" only makes sense right after a fresh install.
            this.ShowLaunchOption = context.Flow == InstallerFlow.Install;

            this.Buttons = new[]
            {
                new DialogButtonSpec(Strings.FinishButton, true, (s, e) => this.Host.Finish(), isDefault: true, isCancel: true),
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
