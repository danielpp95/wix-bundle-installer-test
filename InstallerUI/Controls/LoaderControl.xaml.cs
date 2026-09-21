namespace InstallerUI.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// The percentage/bar/status readout from the loader reference (Assets/examples/laoder.png):
    /// a large percentage number over a thin flat progress bar, with a status line underneath.
    /// Two dependency properties so it's usable from XAML bindings as well as the
    /// code-behind property sets InstallingPage already does.
    /// </summary>
    public partial class LoaderControl : UserControl
    {
        public static readonly DependencyProperty PercentageProperty = DependencyProperty.Register(
            nameof(Percentage),
            typeof(int),
            typeof(LoaderControl),
            new PropertyMetadata(0, OnPercentageChanged));

        public static readonly DependencyProperty StatusTextProperty = DependencyProperty.Register(
            nameof(StatusText),
            typeof(string),
            typeof(LoaderControl),
            new PropertyMetadata(string.Empty, OnStatusTextChanged));

        public LoaderControl()
        {
            this.InitializeComponent();
            this.UpdatePercentageDisplay(0);
        }

        public int Percentage
        {
            get => (int)this.GetValue(PercentageProperty);
            set => this.SetValue(PercentageProperty, value);
        }

        public string StatusText
        {
            get => (string)this.GetValue(StatusTextProperty);
            set => this.SetValue(StatusTextProperty, value);
        }

        private static void OnPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            ((LoaderControl)d).UpdatePercentageDisplay((int)e.NewValue);

        private static void OnStatusTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            ((LoaderControl)d).StatusTextBlock.Text = (string)e.NewValue;

        private void UpdatePercentageDisplay(int percentage)
        {
            var clamped = percentage < 0 ? 0 : percentage > 100 ? 100 : percentage;

            this.PercentageText.Text = clamped + "%";
            this.FillColumn.Width = new GridLength(clamped, GridUnitType.Star);
            this.RemainderColumn.Width = new GridLength(100 - clamped, GridUnitType.Star);
        }
    }
}
