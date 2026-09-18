namespace InstallerUI.Pages
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    public partial class EulaPage : UserControl
    {
        public EulaPage()
        {
            this.InitializeComponent();
            this.LoadEulaFromResource();
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

        private void OnAcceptChanged(object sender, RoutedEventArgs e) =>
            this.AcceptedChanged?.Invoke(this, EventArgs.Empty);
    }
}
