namespace InstallerUI.Pages
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    public partial class EulaPage : BasePage
    {
        public EulaPage()
        {
            this.InitializeComponent();
            this.PageTitle = "End-User legal agreement";
            this.LoadEulaFromResource();
            this.RebuildButtons();
        }

        private void RebuildButtons()
        {
            this.buttons = new[]
            {
                new DialogButtonSpec("Cancel", true, (s, e) => this.Cancel(), isCancel: true),
                new DialogButtonSpec("Prev", true, (s, e) => this.ShowPreviousPage()),
                new DialogButtonSpec("Next", this.IsAccepted, (s, e) => this.ShowNextPage(), isDefault: true),
            };
        }

        public event EventHandler AcceptedChanged;

        public bool IsAccepted => this.AcceptCheckBox.IsChecked == true;

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
            this.RebuildButtons();
            this.AcceptedChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
