namespace InstallerUI.Pages
{
    using System.Windows.Controls;

    public partial class PresentationPage : BasePage
    {
        public PresentationPage()
        {
            InitializeComponent();
            this.PageTitle = "My super app installer";
            this.buttons = new[]
            {
                new DialogButtonSpec("Cancel", true, (s, e) => this.Cancel(), isCancel: true),
                new DialogButtonSpec("Next", true, (s, e) => this.ShowNextPage(), isDefault: true),
            };
        }

        public string Description
        {
            get => this.DescriptionText.Text;
            set => this.DescriptionText.Text = value;
        }
    }
}
