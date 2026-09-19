
namespace InstallerUI.Pages
{
    using System;
    using System.Windows.Controls;


    public class BasePage: UserControl
    {

        public   string PageTitle { get; set; } = string.Empty;

        public Action ShowNextPage { get; set; } = null;
        public Action ShowPreviousPage { get; set; } = null;
        public Action Cancel { get; set; } = null;
        public Action Finish { get; set; } = null;

        internal DialogButtonSpec[] buttons = new DialogButtonSpec[0];
    }
}
