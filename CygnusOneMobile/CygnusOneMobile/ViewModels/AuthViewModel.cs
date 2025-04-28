using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CygnusOneMobile.Services;
using CygnusOneMobile.Views;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

namespace CygnusOneMobile.ViewModels
{
    public class AuthViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly SessionManager _sessionManager;
        
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isAuthenticating;
        private bool _isPasswordHidden = true;
        private string _title = "Login";
        
        // Add IsNotAuthenticating and HasErrorMessage properties that were missing
        public bool IsNotAuthenticating => !IsAuthenticating;
        
        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
        
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        
        public bool IsAuthenticating
        {
            get => _isAuthenticating;
            set => SetProperty(ref _isAuthenticating, value);
        }
        
        public bool IsPasswordHidden
        {
            get => _isPasswordHidden;
            set => SetProperty(ref _isPasswordHidden, value);
        }
        
        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand OpenDebugPageCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        
        public AuthViewModel(AuthService authService, SessionManager sessionManager, DebugLogger? logger = null)
            : base(logger)
        {
            Title = "Login";
            
            // Initialize services
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            
            // Use safer command implementations with explicit exception handling
            LoginCommand = new Command(async () => {
                try {
                    await Login();
                } catch (Exception ex) {
                    Debug.WriteLine($"Login command exception: {ex.Message}");
                    LogError("Login command exception", ex);
                    ErrorMessage = $"Login error: {ex.Message}";
                }
            });
            
            RegisterCommand = new Command(async () => {
                try {
                    await Register();
                } catch (Exception ex) {
                    Debug.WriteLine($"Register command exception: {ex.Message}");
                    LogError("Register command exception", ex);
                    ErrorMessage = $"Registration error: {ex.Message}";
                }
            });
            
            OpenDebugPageCommand = new Command(async () => {
                try {
                    await OpenDebugPage();
                } catch (Exception ex) {
                    Debug.WriteLine($"Debug page navigation exception: {ex.Message}");
                    LogError("Debug page navigation exception", ex);
                    ErrorMessage = $"Navigation error: {ex.Message}";
                }
            });
            
            TogglePasswordVisibilityCommand = new Command(TogglePasswordVisibility);
            
            LogInfo("AuthViewModel initialized");
            Debug.WriteLine("AuthViewModel initialized");
        }
        
        private void TogglePasswordVisibility()
        {
            IsPasswordHidden = !IsPasswordHidden;
            LogInfo($"Password visibility toggled to {(IsPasswordHidden ? "hidden" : "visible")}");
            
            // Make sure to notify for the derived property too
            OnPropertyChanged(nameof(IsNotAuthenticating));
        }
        
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both email and password";
                LogWarning("Login attempt failed: Missing email or password");
                OnPropertyChanged(nameof(HasErrorMessage));
                return;
            }
            
            try
            {
                IsAuthenticating = true;
                OnPropertyChanged(nameof(IsNotAuthenticating));
                ErrorMessage = string.Empty;
                OnPropertyChanged(nameof(HasErrorMessage));
                
                LogInfo($"Attempting login for user: {Email}");
                
                var (success, token, user, errorMessage) = await _authService.LoginAsync(Email, Password);
                
                if (success && !string.IsNullOrEmpty(token) && user != null)
                {
                    LogInfo($"Login successful for user: {user.Email}");
                    
                    // The session is already saved by the AuthService
                    // Navigation will be handled by the AppShell via messaging
                    // No need to navigate here
                }
                else
                {
                    ErrorMessage = errorMessage ?? "Failed to login";
                    OnPropertyChanged(nameof(HasErrorMessage));
                    LogWarning($"Login failed: {ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login error: {ex.Message}";
                OnPropertyChanged(nameof(HasErrorMessage));
                LogError("Login exception", ex);
                Debug.WriteLine($"Login exception: {ex}");
            }
            finally
            {
                IsAuthenticating = false;
                OnPropertyChanged(nameof(IsNotAuthenticating));
            }
        }
        
        private async Task Register()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both email and password to register";
                OnPropertyChanged(nameof(HasErrorMessage));
                LogWarning("Registration attempt failed: Missing email or password");
                return;
            }
            
            try
            {
                IsAuthenticating = true;
                OnPropertyChanged(nameof(IsNotAuthenticating));
                ErrorMessage = string.Empty;
                OnPropertyChanged(nameof(HasErrorMessage));
                
                LogInfo($"Attempting registration for user: {Email}");
                
                var (success, token, user, errorMessage) = await _authService.RegisterAsync(Email, Password);
                
                if (success && !string.IsNullOrEmpty(token) && user != null)
                {
                    LogInfo($"Registration successful for user: {user.Email}");
                    
                    // Navigation will be handled by the AppShell via messaging
                    // No need to navigate here
                }
                else
                {
                    ErrorMessage = errorMessage ?? "Failed to register";
                    OnPropertyChanged(nameof(HasErrorMessage));
                    LogWarning($"Registration failed: {ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration error: {ex.Message}";
                OnPropertyChanged(nameof(HasErrorMessage));
                LogError("Registration exception", ex);
                Debug.WriteLine($"Registration exception: {ex}");
            }
            finally
            {
                IsAuthenticating = false;
                OnPropertyChanged(nameof(IsNotAuthenticating));
            }
        }
        
        private async Task OpenDebugPage()
        {
            LogInfo("Opening Debug Page");
            
            try {
                // Use the route that's defined in the shell
                await Shell.Current.GoToAsync("//DebugRoute");
            }
            catch (Exception ex) {
                LogError("Failed to navigate to debug page", ex);
                ErrorMessage = $"Navigation error: {ex.Message}";
                OnPropertyChanged(nameof(HasErrorMessage));
            }
        }
    }
}