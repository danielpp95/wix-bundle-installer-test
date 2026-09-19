namespace InstallerUI.Pages
{
    using System.Windows.Controls;

    public partial class ErrorPage : BasePage
    {
        public ErrorPage()
        {
            this.InitializeComponent();

            this.PageTitle = "Error";
            this.buttons = new[]
            {
                new DialogButtonSpec("Close", true, (s, e) => this.Finish(), isDefault: true, isCancel: true),
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
