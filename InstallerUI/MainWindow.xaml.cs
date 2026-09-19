namespace InstallerUI
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
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

        public void Navigate(BasePage page)
        {
            this.PageTitleText.Text = page.PageTitle;
            this.PageHost.Content = page;

            this.ButtonBar.Children.Clear();
            foreach (var spec in page.buttons)
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
