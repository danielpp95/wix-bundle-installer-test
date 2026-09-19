namespace InstallerUI.Pages
{
    using InstallerUI.Models;

    public partial class ErrorPage : BasePage
    {
        public ErrorPage(InstallerContext context)
        {
            this.InitializeComponent();

            this.PageTitle = context.Flow switch
            {
                InstallerFlow.Uninstall => Strings.UninstallFailedTitle,
                InstallerFlow.Repair => Strings.RepairFailedTitle,
                _ => Strings.SetupFailedTitle,
            };

            this.Buttons = new[]
            {
                new DialogButtonSpec(Strings.CloseButton, true, (s, e) => this.Host.Finish(), isDefault: true, isCancel: true),
            };
        }

        public string Message
        {
            get => this.MessageText.Text;
            set => this.MessageText.Text = value;
        }

        public string Details
        {
            get => this.DetailsTextBox.Text;
            set => this.DetailsTextBox.Text = value;
        }
    }
}
