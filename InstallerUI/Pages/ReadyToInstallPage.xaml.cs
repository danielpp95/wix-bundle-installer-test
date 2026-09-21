namespace InstallerUI.Pages
{
    using System.Collections.Generic;
    using InstallerUI.Models;

    public partial class ReadyToInstallPage : BasePage
    {
        public ReadyToInstallPage(InstallerContext context)
        {
            this.InitializeComponent();

            string actionLabel;
            switch (context.Flow)
            {
                case InstallerFlow.Uninstall:
                    this.PageTitle = Strings.ReadyToUninstallTitle;
                    this.Message = Strings.ReadyToUninstallMessage;
                    actionLabel = Strings.UninstallButton;
                    this.StepLabels = Strings.UninstallSteps;
                    this.StepIndex = 0;
                    break;
                case InstallerFlow.Repair:
                    this.PageTitle = Strings.ReadyToRepairTitle;
                    this.Message = Strings.ReadyToRepairMessage;
                    actionLabel = Strings.RepairButton;
                    this.StepLabels = Strings.RepairSteps;
                    this.StepIndex = 0;
                    break;
                default:
                    this.PageTitle = Strings.ReadyToInstallTitle;
                    this.Message = Strings.ReadyToInstallMessage;
                    actionLabel = Strings.InstallButton;
                    this.StepLabels = Strings.InstallSteps;
                    this.StepIndex = 3;
                    break;
            }

            var buttons = new List<DialogButtonSpec>
            {
                new DialogButtonSpec(Strings.CancelButton, true, (s, e) => this.Host.Cancel(), isCancel: true),
            };

            // Uninstall/repair skip the presentation/EULA/install-path pages entirely, so
            // there is nothing to go "back" to.
            if (context.Flow == InstallerFlow.Install)
            {
                buttons.Add(new DialogButtonSpec(Strings.PrevButton, true, (s, e) => this.Host.ShowPreviousPage()));
            }

            buttons.Add(new DialogButtonSpec(actionLabel, true, (s, e) => this.Host.ShowNextPage(), isDefault: true));

            this.Buttons = buttons.ToArray();
        }

        public string Message
        {
            get => this.MessageText.Text;
            set => this.MessageText.Text = value;
        }
    }
}
