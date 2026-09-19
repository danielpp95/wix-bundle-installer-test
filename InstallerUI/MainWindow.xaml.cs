namespace InstallerUI
{
    using System.Windows;
    using System.Windows.Controls;
    using InstallerUI.Pages;

    public class DialogButtonSpec
    {
        public DialogButtonSpec(string text, bool isEnabled, RoutedEventHandler onClick, bool isDefault = false, bool isCancel = false)
        {
            this.Text = text;
            this.IsEnabled = isEnabled;
            this.OnClick = onClick;
            this.IsDefault = isDefault;
            this.IsCancel = isCancel;
        }

        public string Text { get; }

        public bool IsEnabled { get; }

        public RoutedEventHandler OnClick { get; }

        public bool IsDefault { get; }

        public bool IsCancel { get; }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Burn's Apply() rejects a null window handle even for a fully silent install, so
        /// a Display.None/Embedded run still needs a real HWND - it just must never be
        /// visible. Shrinking/hiding it (rather than skipping window creation) also means
        /// there is exactly one startup code path for every Display level.
        /// </summary>
        public void HideForUnattendedRun()
        {
            this.WindowStyle = WindowStyle.None;
            this.ShowInTaskbar = false;
            this.ResizeMode = ResizeMode.NoResize;
            this.Width = 0;
            this.Height = 0;
            this.Left = -32000;
            this.Top = -32000;
        }

        public void Navigate(BasePage page)
        {
            this.PageTitleText.Text = page.PageTitle;
            this.PageHost.Content = page;

            this.ButtonBar.Children.Clear();
            foreach (var spec in page.Buttons)
            {
                var button = new Button
                {
                    Content = spec.Text,
                    IsEnabled = spec.IsEnabled,
                    IsDefault = spec.IsDefault,
                    IsCancel = spec.IsCancel,
                    Style = (Style)this.FindResource("DialogButton"),
                };
                button.Click += spec.OnClick;
                this.ButtonBar.Children.Add(button);
            }
        }
    }
}
