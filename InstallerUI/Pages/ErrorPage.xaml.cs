namespace InstallerUI.Pages
{
    using System.Windows.Controls;

    public partial class ErrorPage : UserControl
    {
        public ErrorPage()
        {
            this.InitializeComponent();
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
