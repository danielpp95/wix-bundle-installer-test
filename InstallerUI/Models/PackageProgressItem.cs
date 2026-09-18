namespace InstallerUI.Models
{
    using System.ComponentModel;

    public enum PackageStepStatus
    {
        Waiting,
        Installing,
        Done,
        Failed,
    }

    public class PackageProgressItem : INotifyPropertyChanged
    {
        private PackageStepStatus _status = PackageStepStatus.Waiting;

        public PackageProgressItem(string packageId, string displayName)
        {
            this.PackageId = packageId;
            this.DisplayName = displayName;
        }

        public string PackageId { get; }

        public string DisplayName { get; }

        public PackageStepStatus Status
        {
            get => this._status;
            set
            {
                if (this._status != value)
                {
                    this._status = value;
                    this.OnPropertyChanged(nameof(this.Status));
                    this.OnPropertyChanged(nameof(this.StatusText));
                }
            }
        }

        public string StatusText
        {
            get
            {
                switch (this._status)
                {
                    case PackageStepStatus.Installing:
                        return "Installing";
                    case PackageStepStatus.Done:
                        return "Done";
                    case PackageStepStatus.Failed:
                        return "Failed";
                    default:
                        return "Waiting";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
