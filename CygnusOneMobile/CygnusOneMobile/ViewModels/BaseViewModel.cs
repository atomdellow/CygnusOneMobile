using CygnusOneMobile.Services;
using CygnusOneMobile.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CygnusOneMobile.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected readonly DebugLogger? _logger;
        private bool _isBusy = false;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public BaseViewModel(DebugLogger? logger = null)
        {
            _logger = logger;
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action? onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void LogDebug(string message)
        {
            _logger?.Log(message, LogLevel.Debug);
        }

        protected void LogInfo(string message)
        {
            _logger?.Log(message, LogLevel.Info);
        }

        protected void LogWarning(string message)
        {
            _logger?.Log(message, LogLevel.Warning);
        }

        protected void LogError(string message)
        {
            _logger?.Log(message, LogLevel.Error);
        }

        protected void LogError(string context, Exception exception)
        {
            _logger?.LogError(context, exception.Message);
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}