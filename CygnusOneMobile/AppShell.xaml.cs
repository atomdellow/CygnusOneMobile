using CygnusOneMobile.Services;
using CygnusOneMobile.Views;

namespace CygnusOneMobile
{
    public partial class AppShell : Shell
    {
        private readonly SessionManager _sessionManager;
        
        public AppShell()
        {
            InitializeComponent();
            
            _sessionManager = new SessionManager();
            
            // Register routes
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute("articles", typeof(ArticlesPage));
            
            // Check authentication and redirect if needed
            CheckAuthenticationAsync();
        }
        
        private async void CheckAuthenticationAsync()
        {
            // Give UI time to initialize
            await Task.Delay(100);
            
            // Check if user is authenticated
            bool isAuthenticated = await _sessionManager.IsAuthenticatedAsync();
            
            if (isAuthenticated)
            {
                // User is authenticated, navigate to main app
                await Shell.Current.GoToAsync("//main/articles");
            }
            else
            {
                // User is not authenticated, navigate to login
                await Shell.Current.GoToAsync("//login");
            }
        }
    }
}