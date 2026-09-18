namespace InstallerUI.Pages
{
    using System.Windows.Controls;

    public partial class PresentationPage : UserControl
    {
        public PresentationPage()
        {
            this.InitializeComponent();
        }

        public string Description
        {
            get => this.DescriptionText.Text;
            set => this.DescriptionText.Text = value;
        }
    }
}
