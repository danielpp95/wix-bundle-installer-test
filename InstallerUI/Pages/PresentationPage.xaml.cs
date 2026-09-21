namespace InstallerUI.Pages
{
    public partial class PresentationPage : BasePage
    {
        public PresentationPage()
        {
            this.InitializeComponent();
            this.PageTitle = Strings.PresentationTitle;
            this.Description = Strings.PresentationDescription;
            this.StepLabels = Strings.InstallSteps;
            this.StepIndex = 0;
            this.Buttons = new[]
            {
                new DialogButtonSpec(Strings.CancelButton, true, (s, e) => this.Host.Cancel(), isCancel: true),
                new DialogButtonSpec(Strings.NextButton, true, (s, e) => this.Host.ShowNextPage(), isDefault: true),
            };
        }

        public string Description
        {
            get => this.DescriptionText.Text;
            set => this.DescriptionText.Text = value;
        }
    }
}
