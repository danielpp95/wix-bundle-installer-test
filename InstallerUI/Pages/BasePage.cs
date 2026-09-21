namespace InstallerUI.Pages
{
    using System;
    using System.Windows.Controls;

    /// <summary>
    /// What a page is allowed to ask the shell to do. Pages depend on this interface
    /// instead of on InstallerBootstrapperApplication directly, so a page can be
    /// exercised without a live Burn engine behind it.
    /// </summary>
    public interface INavigationHost
    {
        void ShowNextPage();

        void ShowPreviousPage();

        void Cancel();

        void Finish();

        /// <summary>Re-renders the button bar for a page whose own Buttons changed (e.g. a checkbox toggled) without navigating away from it.</summary>
        void RefreshButtons(BasePage page);
    }

    public abstract class BasePage : UserControl
    {
        public string PageTitle { get; protected set; } = string.Empty;

        public DialogButtonSpec[] Buttons { get; protected set; } = Array.Empty<DialogButtonSpec>();

        public INavigationHost Host { get; set; }

        /// <summary>Step labels for the wizard's step indicator, for whichever flow this page belongs to (Install vs. Uninstall/Repair).</summary>
        public string[] StepLabels { get; protected set; } = Array.Empty<string>();

        /// <summary>This page's 0-based position within StepLabels.</summary>
        public int StepIndex { get; protected set; }
    }
}
