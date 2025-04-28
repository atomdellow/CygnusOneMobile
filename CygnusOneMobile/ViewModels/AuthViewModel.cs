using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Text.RegularExpressions;
using CygnusOneMobile.Services;
using CygnusOneMobile.Views;
using CygnusOneMobile.Models;
using Microsoft.Maui.Controls;

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
        private User _currentUser;
        
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        
        public string Email
        {
            get => _email;
            set 
            { 
                SetProperty(ref _email, value);
                // Force command re-evaluation
                (LoginCommand as Command)?.ChangeCanExecute();
                (RegisterCommand as Command)?.ChangeCanExecute();
            }
        }
        
        public string Password
        {
            get => _password;
            set 
            { 
                SetProperty(ref _password, value);
                // Force command re-evaluation
                (LoginCommand as Command)?.ChangeCanExecute();
                (RegisterCommand as Command)?.ChangeCanExecute();
            }
        }
        
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        
        public bool IsAuthenticating
        {
            get => _isAuthenticating;
            set 
            { 
                SetProperty(ref _isAuthenticating, value);
                // Force command re-evaluation
                (LoginCommand as Command)?.ChangeCanExecute();
                (RegisterCommand as Command)?.ChangeCanExecute();
            }
        }
        
        public bool IsPasswordHidden
        {
            get => _isPasswordHidden;
            set => SetProperty(ref _isPasswordHidden, value);
        }

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                SetProperty(ref _currentUser, value);
                OnPropertyChanged(nameof(IsAuthenticated));
            }
        }
        
        public bool IsAuthenticated => CurrentUser != null;
        
        public ICommand LoginCommand { get; private set; }
        public ICommand RegisterCommand { get; private set; }
        public ICommand OpenDebugPageCommand { get; private set; }
        public ICommand TogglePasswordVisibilityCommand { get; private set; }
        
        public AuthViewModel(AuthService authService, SessionManager sessionManager, DebugLogger? logger = null)
            : base(logger)
        {
            Title = "Login";
            
            // Initialize services
            _authService = authService;
            _sessionManager = sessionManager;
            
            // Create commands with CanExecute handlers for validation
            LoginCommand = new Command(
                async () => await Login(), 
                () => !IsAuthenticating && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password)
            );
            
            RegisterCommand = new Command(
                async () => await Register(), 
                () => !IsAuthenticating && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password)
            );
            
            OpenDebugPageCommand = new Command(
                async () => await OpenDebugPage()
            );
            
            TogglePasswordVisibilityCommand = new Command(
                TogglePasswordVisibility
            );
            
            LogInfo("AuthViewModel initialized");
            
            // Check for existing session on startup
            Task.Run(async () => await CheckExistingSession());
        }
        
        private async Task CheckExistingSession()
        {
            try
            {
                var user = _sessionManager.GetCurrentSession();
                if (user != null)
                {
                    CurrentUser = user;
                    LogInfo($"Existing session found for user: {user.Email}");
                }
            }
            catch (Exception ex)
            {
                LogError("Error checking existing session", ex);
            }
        }
        
        private void TogglePasswordVisibility()
        {
            IsPasswordHidden = !IsPasswordHidden;
            LogInfo($"Password visibility toggled to {(IsPasswordHidden ? "hidden" : "visible")}");
        }
        
        private async Task Login()
        {
            if (!ValidateForm())
            {
                return;
            }
            
            try
            {
                IsAuthenticating = true;
                ErrorMessage = string.Empty;
                
                LogInfo($"Attempting login for user: {Email}");
                
                var (success, token, user, errorMessage) = await _authService.LoginAsync(Email, Password);
                
                if (success && !string.IsNullOrEmpty(token) && user != null)
                {
                    LogInfo($"Login successful for user: {user.Email}");
                    
                    // Save session
                    await _sessionManager.SetSessionAsync(token, user);
                    CurrentUser = user;
                    
                    // Navigate to the main page - the AppShell will handle the navigation based on auth status
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    ErrorMessage = errorMessage ?? "Failed to login";
                    LogWarning($"Login failed: {ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login error: {ex.Message}";
                LogError("Login exception", ex);
            }
            finally
            {
                IsAuthenticating = false;
            }
        }
        
        private async Task Register()
        {
            if (!ValidateForm())
            {
                return;
            }
            
            try
            {
                IsAuthenticating = true;
                ErrorMessage = string.Empty;
                
                LogInfo($"Attempting registration for user: {Email}");
                
                var (success, token, user, errorMessage) = await _authService.RegisterAsync(Email, Password);
                
                if (success && !string.IsNullOrEmpty(token) && user != null)
                {
                    LogInfo($"Registration successful for user: {user.Email}");
                    
                    // Save session
                    await _sessionManager.SetSessionAsync(token, user);
                    CurrentUser = user;
                    
                    // Navigate to the main page - the AppShell will handle the navigation
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    ErrorMessage = errorMessage ?? "Failed to register";
                    LogWarning($"Registration failed: {ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration error: {ex.Message}";
                LogError("Registration exception", ex);
            }
            finally
            {
                IsAuthenticating = false;
            }
        }
        
        private async Task OpenDebugPage()
        {
            try
            {
                LogInfo("Opening Debug Page");
                await Shell.Current.GoToAsync("DebugRoute");
            }
            catch (Exception ex)
            {
                LogError("Error navigating to Debug Page", ex);
                ErrorMessage = $"Navigation error: {ex.Message}";
            }
        }
        
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email is required.";
                return false;
            }
            
            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Please enter a valid email address.";
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Password is required.";
                return false;
            }
            
            if (Password.Length < 6)
            {
                ErrorMessage = "Password must be at least 6 characters.";
                return false;
            }
            
            return true;
        }
        
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
                
            try
            {
                // Use regular expression for basic email validation
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}