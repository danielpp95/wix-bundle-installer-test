namespace InstallerUI.Pages
{
    using System;
    using System.Windows;
    using System.Windows.Documents;

    public partial class EulaPage : BasePage
    {
        public EulaPage()
        {
            this.InitializeComponent();
            this.PageTitle = Strings.EulaTitle;
            this.LoadEulaFromResource();
            this.RebuildButtons();
        }

        public bool IsAccepted => this.AcceptCheckBox.IsChecked == true;

        private void RebuildButtons()
        {
            this.Buttons = new[]
            {
                new DialogButtonSpec(Strings.CancelButton, true, (s, e) => this.Host.Cancel(), isCancel: true),
                new DialogButtonSpec(Strings.PrevButton, true, (s, e) => this.Host.ShowPreviousPage()),
                new DialogButtonSpec(Strings.NextButton, this.IsAccepted, (s, e) => this.Host.ShowNextPage(), isDefault: true),
            };
        }

        private void LoadEulaFromResource()
        {
            var uri = new Uri("pack://application:,,,/InstallerUI;component/Assets/Eula.rtf", UriKind.Absolute);
            var info = Application.GetResourceStream(uri);
            if (info == null)
            {
                return;
            }

            using (var stream = info.Stream)
            {
                var range = new TextRange(this.EulaText.Document.ContentStart, this.EulaText.Document.ContentEnd);
                range.Load(stream, DataFormats.Rtf);
            }
        }

        private void OnAcceptChanged(object sender, RoutedEventArgs e)
        {
            // The Next button's enabled state depends on the checkbox, so every button in
            // the bar has to be rebuilt (and the host asked to refresh) whenever it flips.
            this.RebuildButtons();
            this.Host?.RefreshButtons(this);
        }
    }
}
