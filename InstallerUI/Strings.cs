namespace InstallerUI
{
    /// <summary>
    /// Every string the wizard shows to a user, in one place. This is a plain class rather
    /// than a full .resx/satellite-assembly setup so a team can start here and graduate to
    /// real localization later without touching call sites - everything already goes
    /// through Strings.*.
    /// </summary>
    internal static class Strings
    {
        public const string PresentationTitle = "My super app installer";
        public const string PresentationDescription = "This is a generic installer for windows apps using wix";

        public const string EulaTitle = "End-User legal agreement";
        public const string EulaAcceptLabel = "Accept terms and conditions";

        public const string InstallPathTitle = "Installation folder";

        public const string ReadyToInstallTitle = "Ready to install";
        public const string ReadyToUninstallTitle = "Ready to uninstall";
        public const string ReadyToRepairTitle = "Ready to repair";
        public const string ReadyToInstallMessage = "Click install to begin the installation.";
        public const string ReadyToUninstallMessage = "Click uninstall to remove the application.";
        public const string ReadyToRepairMessage = "Click repair to fix the current installation.";

        public const string InstallingTitle = "Installing";
        public const string UninstallingTitle = "Uninstalling";
        public const string RepairingTitle = "Repairing";

        public const string InstalledTitle = "Installed successfully";
        public const string UninstalledTitle = "Uninstalled";
        public const string RepairedTitle = "Repaired successfully";
        public const string InstalledMessage = "The app was installed successfully";
        public const string UninstalledMessage = "The app was uninstalled successfully";
        public const string RepairedMessage = "The app was repaired successfully";
        public const string LaunchOnFinishLabel = "Open the app on finish";

        public const string SetupFailedTitle = "Setup failed";
        public const string UninstallFailedTitle = "Uninstall failed";
        public const string RepairFailedTitle = "Repair failed";
        public const string DetectFailedMessage = "Detecting the current state of the machine failed.";
        public const string PlanFailedMessage = "Planning the operation failed.";
        public const string CanceledMessage = "The operation was canceled.";
        public const string InstallFailedMessage = "The installation failed.";
        public const string UninstallFailedMessage = "The uninstall failed.";
        public const string RepairFailedMessage = "The repair failed.";

        public const string CancelButton = "Cancel";
        public const string PrevButton = "Prev";
        public const string NextButton = "Next";
        public const string InstallButton = "Install";
        public const string UninstallButton = "Uninstall";
        public const string RepairButton = "Repair";
        public const string FinishButton = "Finish";
        public const string CloseButton = "Close";
        public const string BrowseButton = "Browse";
    }
}
