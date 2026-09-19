namespace InstallerUI.Models
{
    using WixToolset.BootstrapperApplicationApi;

    public enum InstallerFlow
    {
        Install,
        Uninstall,
        Repair,
    }

    /// <summary>
    /// The single source of truth for "what is this run doing", derived once from the
    /// command-line action Burn handed us. Pages take this in their constructor instead
    /// of reaching into the bootstrapper application for a static flag.
    /// </summary>
    public class InstallerContext
    {
        public InstallerContext(LaunchAction action)
        {
            this.Action = action;

            if (action == LaunchAction.Uninstall || action == LaunchAction.UnsafeUninstall)
            {
                this.Flow = InstallerFlow.Uninstall;
            }
            else if (action == LaunchAction.Repair || action == LaunchAction.Modify)
            {
                this.Flow = InstallerFlow.Repair;
            }
            else
            {
                this.Flow = InstallerFlow.Install;
            }
        }

        public LaunchAction Action { get; }

        public InstallerFlow Flow { get; }

        public bool IsUninstall => this.Flow == InstallerFlow.Uninstall;

        public bool IsMaintenance => this.Flow != InstallerFlow.Install;
    }
}
