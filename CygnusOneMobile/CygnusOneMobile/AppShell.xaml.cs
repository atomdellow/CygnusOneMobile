using CygnusOneMobile.Services;
using CygnusOneMobile.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;

namespace CygnusOneMobile;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AppShell : Shell
{
    private readonly SessionManager _sessionManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly DebugLogger? _logger;
    private bool _isInitialized = false;

    public AppShell(SessionManager sessionManager, IServiceProvider serviceProvider, DebugLogger? logger = null)
    {
        InitializeComponent();
        _sessionManager = sessionManager;
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // Subscribe to authentication changes using WeakReferenceMessenger
        WeakReferenceMessenger.Default.Register<string>(this, (recipient, message) => {
            if (message == "UserLoggedIn" || message == "UserLoggedOut")
            {
                _logger?.Log($"Authentication message received: {message}", LogLevel.Info);
                UpdateUIBasedOnAuthenticationStatus();
            }
        });
        
        // Wait until shell is loaded before attempting navigation
        this.Loaded += (s, e) => {
            _logger?.Log("AppShell loaded, checking authentication status", LogLevel.Info);
            _isInitialized = true;
            // Check login status when shell is loaded
            UpdateUIBasedOnAuthenticationStatus();
        };

        // Set initial visibility state without navigation yet
        bool isLoggedIn = _sessionManager.IsLoggedIn;
        _logger?.Log($"Initial AppShell setup - IsLoggedIn: {isLoggedIn}", LogLevel.Info);
        
        // We need to call this directly since we can't wait for the Loaded event during initialization
        InitializeUIVisibility();
        
        _logger?.Log("AppShell initialized with SessionManager", LogLevel.Info);
        Debug.WriteLine("AppShell initialized with SessionManager");
    }
    
    private void InitializeUIVisibility()
    {
        bool isLoggedIn = _sessionManager.IsLoggedIn;
        _logger?.Log($"Setting UI visibility - IsLoggedIn: {isLoggedIn}", LogLevel.Debug);

        // Use FindByName to safely access XAML elements
        if (this.FindByName("MainTabBar") is TabBar mainTabBar)
            mainTabBar.IsVisible = isLoggedIn;
            
        if (this.FindByName("LoginContent") is ShellContent loginContent)
            loginContent.IsVisible = !isLoggedIn;
    }

    private void UpdateUIBasedOnAuthenticationStatus()
    {
        try 
        {
            bool isLoggedIn = _sessionManager.IsLoggedIn;
            _logger?.Log($"UpdateUIBasedOnAuthenticationStatus called - IsLoggedIn: {isLoggedIn}", LogLevel.Info);

            // Set UI element visibility
            if (this.FindByName("MainTabBar") is TabBar mainTabBar)
                mainTabBar.IsVisible = isLoggedIn;
                
            if (this.FindByName("LoginContent") is ShellContent loginContent)
                loginContent.IsVisible = !isLoggedIn;

            // Only navigate if Shell is fully initialized
            if (Current != null && _isInitialized)
            {
                if (isLoggedIn)
                {
                    // Navigate to the articles tab when logged in
                    _logger?.Log("Authenticated: Navigating to Articles tab", LogLevel.Info);
                    Current.Dispatcher.Dispatch(async () => {
                        try {
                            await Current.GoToAsync("//ArticlesPage");
                        }
                        catch (Exception ex) {
                            _logger?.LogException(ex, "Error during navigation to ArticlesPage");
                            Debug.WriteLine($"Navigation exception: {ex.Message}");
                        }
                    });
                }
                else
                {
                    // Navigate to login page when not logged in
                    _logger?.Log("Not authenticated: Navigating to Login page", LogLevel.Info);
                    Current.Dispatcher.Dispatch(async () => {
                        try {
                            // Always make sure the LoginContent is visible
                            if (this.FindByName("LoginContent") is ShellContent loginContent)
                                loginContent.IsVisible = true;
                                
                            await Current.GoToAsync("//LoginPage");
                        }
                        catch (Exception ex) {
                            _logger?.LogException(ex, "Error during navigation to LoginPage");
                            Debug.WriteLine($"Navigation exception: {ex.Message}");
                        }
                    });
                }
            }
            else
            {
                _logger?.Log("Shell not initialized yet or Current is null", LogLevel.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogException(ex, "Error in UpdateUIBasedOnAuthenticationStatus");
            Debug.WriteLine($"Error in UpdateUIBasedOnAuthenticationStatus: {ex.Message}");
        }
    }
}
