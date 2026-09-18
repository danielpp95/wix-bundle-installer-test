namespace InstallerUI.Pages
{
    using System.Windows.Controls;

    public partial class ReadyToInstallPage : UserControl
    {
        public ReadyToInstallPage()
        {
            this.InitializeComponent();
        }

        public string Message
        {
            get => this.MessageText.Text;
            set => this.MessageText.Text = value;
        }
    }
}
