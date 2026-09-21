namespace InstallerUI.Controls
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>One rendered circle+label in the step indicator. Immutable - rebuilt wholesale whenever Steps or CurrentStep changes.</summary>
    public class StepIndicatorItem
    {
        public int Number { get; set; }

        public string Label { get; set; }

        public bool IsCurrent { get; set; }

        public bool ShowLeftLine { get; set; }

        public bool ShowRightLine { get; set; }
    }

    /// <summary>
    /// The step row from the wizard reference (Assets/examples/wizard.png): numbered
    /// circles joined by a line, current step highlighted, label under each. Steps is the
    /// full label list for whichever flow is running (Install vs. Uninstall/Repair);
    /// CurrentStep is the 0-based index of the page being shown.
    /// </summary>
    public partial class StepIndicatorControl : UserControl
    {
        public static readonly DependencyProperty StepsProperty = DependencyProperty.Register(
            nameof(Steps),
            typeof(string[]),
            typeof(StepIndicatorControl),
            new PropertyMetadata(new string[0], (d, e) => ((StepIndicatorControl)d).Rebuild()));

        public static readonly DependencyProperty CurrentStepProperty = DependencyProperty.Register(
            nameof(CurrentStep),
            typeof(int),
            typeof(StepIndicatorControl),
            new PropertyMetadata(0, (d, e) => ((StepIndicatorControl)d).Rebuild()));

        public StepIndicatorControl()
        {
            this.InitializeComponent();
        }

        public string[] Steps
        {
            get => (string[])this.GetValue(StepsProperty);
            set => this.SetValue(StepsProperty, value);
        }

        public int CurrentStep
        {
            get => (int)this.GetValue(CurrentStepProperty);
            set => this.SetValue(CurrentStepProperty, value);
        }

        private void Rebuild()
        {
            var steps = this.Steps;
            var items = new List<StepIndicatorItem>(steps.Length);

            for (var i = 0; i < steps.Length; i++)
            {
                items.Add(new StepIndicatorItem
                {
                    Number = i + 1,
                    Label = steps[i],
                    IsCurrent = i == this.CurrentStep,
                    ShowLeftLine = i > 0,
                    ShowRightLine = i < steps.Length - 1,
                });
            }

            this.StepsList.ItemsSource = items;
        }
    }
}
