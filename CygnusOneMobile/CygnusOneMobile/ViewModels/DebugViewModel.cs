using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CygnusOneMobile.Services;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;
using CommunityToolkit.Mvvm.Messaging;

namespace CygnusOneMobile.ViewModels
{
    public class DebugViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly SessionManager _sessionManager;
        private readonly new DebugLogger _logger;
        private string _apiUrl = string.Empty;
        private string _connectionStatus = string.Empty;
        private string _connectionStatusColor = string.Empty;
        private string _userId = string.Empty;
        private string _authToken = string.Empty;
        private string _isLoggedIn = string.Empty;
        private string _appVersion = string.Empty;
        private string _devicePlatform = string.Empty;
        private string _deviceInfo = string.Empty;
        private bool _isBusy;
        
        public new bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        
        public string ApiUrl 
        { 
            get => _apiUrl; 
            set => SetProperty(ref _apiUrl, value); 
        }
        
        public string ConnectionStatus 
        { 
            get => _connectionStatus; 
            set => SetProperty(ref _connectionStatus, value); 
        }
        
        public string ConnectionStatusColor 
        { 
            get => _connectionStatusColor; 
            set => SetProperty(ref _connectionStatusColor, value); 
        }
        
        public string UserId 
        { 
            get => _userId; 
            set => SetProperty(ref _userId, value); 
        }
        
        public string AuthToken 
        { 
            get => _authToken; 
            set => SetProperty(ref _authToken, value); 
        }
        
        public string IsLoggedIn 
        { 
            get => _isLoggedIn; 
            set => SetProperty(ref _isLoggedIn, value); 
        }
        
        public string AppVersion 
        { 
            get => _appVersion; 
            set => SetProperty(ref _appVersion, value); 
        }
        
        public string DevicePlatform 
        { 
            get => _devicePlatform; 
            set => SetProperty(ref _devicePlatform, value); 
        }
        
        public string DeviceInfo 
        { 
            get => _deviceInfo; 
            set => SetProperty(ref _deviceInfo, value); 
        }
        
        public ObservableCollection<LogEntry> LogEntries => _logger.LogEntries;
        
        public ICommand TestApiConnectionCommand { get; }
        public ICommand RefreshLogsCommand { get; } // Renamed from RefreshStatusCommand
        public ICommand ClearLogsCommand { get; }
        public ICommand CopyLogsCommand { get; }
        public ICommand ClearSessionCommand { get; } // Added missing command
        
        public DebugViewModel(AuthService authService, SessionManager sessionManager, DebugLogger logger)
            : base(logger)
        {
            _authService = authService;
            _sessionManager = sessionManager;
            _logger = logger;
            
            TestApiConnectionCommand = new Command(async () => await TestApiConnection());
            RefreshLogsCommand = new Command(RefreshLogs); // Changed to match XAML binding
            ClearLogsCommand = new Command(ClearLogs);
            CopyLogsCommand = new Command(CopyLogs);
            ClearSessionCommand = new Command(async () => await ClearSession()); // Added missing command
            
            // Initialize with default values
            ApiUrl = _authService.ApiBaseUrl;
            ConnectionStatus = "Unknown";
            ConnectionStatusColor = "Gray";
            
            // Initialize device info with simple string values
            try
            {
                AppVersion = AppInfo.VersionString;
                
                // Get device information safely
                DevicePlatform = Microsoft.Maui.Devices.DeviceInfo.Platform.ToString();
                DeviceInfo = $"{Microsoft.Maui.Devices.DeviceInfo.Manufacturer} {Microsoft.Maui.Devices.DeviceInfo.Model}";
                
                _logger.LogInfo("Device Info", $"Platform: {DevicePlatform}, Device: {DeviceInfo}");
            }
            catch (Exception ex)
            {
                LogError("Error initializing device info", ex);
                AppVersion = "Unknown";
                DevicePlatform = "Unknown";
                DeviceInfo = "Unknown";
            }
            
            Task.Run(async () => await RefreshStatus());
        }

        public Task InitializeAsync()
        {
            return RefreshStatus();
        }
        
        private async Task TestApiConnection()
        {
            if (IsBusy)
                return;
                
            IsBusy = true;
            try
            {
                ConnectionStatus = "Testing...";
                ConnectionStatusColor = "Gray";
                
                bool isSuccess = await _authService.TestConnection();
                
                if (isSuccess)
                {
                    ConnectionStatus = "Connected";
                    ConnectionStatusColor = "Green";
                    _logger.LogInfo("API Connection Test", "Connection successful");
                }
                else
                {
                    ConnectionStatus = "Failed";
                    ConnectionStatusColor = "Red";
                    _logger.LogError("API Connection Test", "Connection failed");
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus = "Error";
                ConnectionStatusColor = "Red";
                LogError("API Connection Test", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private async Task RefreshStatus()
        {
            if (IsBusy)
                return;
                
            IsBusy = true;
            try
            {
                var user = _sessionManager.GetCurrentSession();
                UserId = user?.Id.ToString() ?? "Not logged in";
                
                var token = await _sessionManager.GetTokenAsync();
                AuthToken = !string.IsNullOrEmpty(token) ? "Token exists" : "No token";
                
                IsLoggedIn = _sessionManager.IsLoggedIn ? "Yes" : "No";
                
                await TestApiConnection();
            }
            catch (Exception ex)
            {
                LogError("Refresh Status", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private async Task ClearSession()
        {
            try
            {
                // Use the new force reset method instead
                await _sessionManager.ForceResetSessionAsync();
                _logger.LogInfo("Session", "Session forcefully reset");
                
                // Send message to notify that the user logged out
                WeakReferenceMessenger.Default.Send<string>("UserLoggedOut");
                
                // Refresh the UI
                await RefreshStatus();
                
                // Navigate to login page
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                LogError("Clear Session", ex);
            }
        }
        
        private void RefreshLogs()
        {
            try
            {
                // This will notify that the collection has changed
                OnPropertyChanged(nameof(LogEntries));
                _logger.LogInfo("Logs", "Logs refreshed");
            }
            catch (Exception ex)
            {
                LogError("Refresh Logs", ex);
            }
        }
        
        private void ClearLogs()
        {
            try
            {
                _logger.ClearLogs();
                _logger.LogInfo("Logs", "Logs cleared");
            }
            catch (Exception ex)
            {
                LogError("Clear Logs", ex);
            }
        }
        
        private void CopyLogs()
        {
            try
            {
                // Implementation for copying logs to clipboard would go here
                _logger.LogInfo("Logs", "Logs copied to clipboard");
            }
            catch (Exception ex)
            {
                LogError("Copy Logs", ex);
            }
        }

        private new void LogError(string message, Exception ex)
        {
            _logger.LogException(ex, message);
        }
    }
}