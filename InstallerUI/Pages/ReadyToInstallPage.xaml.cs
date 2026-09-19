namespace InstallerUI.Pages
{
    using System.Collections.Generic;
    using System.Windows.Controls;

    public partial class ReadyToInstallPage : BasePage
    {
        public ReadyToInstallPage()
        {
            this.InitializeComponent();

            var isUninstall = InstallerBootstrapperApplication.IsUninstall;

            this.PageTitle = isUninstall ? "Ready to uninstall" : "Ready to install";
            this.Message = isUninstall
                ? "Click uninstall to remove the application."
                : "Click install to begin the installation.";

            var buttonList = new List<DialogButtonSpec>
            {
                new DialogButtonSpec("Cancel", true, (s, e) => this.Cancel(), isCancel: true),
            };

            if (!isUninstall)
            {
                buttonList.Add(new DialogButtonSpec("Prev", true, (s, e) => this.ShowPreviousPage()));
            }

            buttonList.Add(new DialogButtonSpec(isUninstall ? "Uninstall" : "Install", true, (s, e) => this.ShowNextPage(), isDefault: true));

            this.buttons = buttonList.ToArray();
        }

        public string Message
        {
            get => this.MessageText.Text;
            set => this.MessageText.Text = value;
        }
    }
}
