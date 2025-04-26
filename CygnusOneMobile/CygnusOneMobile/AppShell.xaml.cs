using CygnusOneMobile.Views;

namespace CygnusOneMobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Register routes for navigation
            Routing.RegisterRoute(nameof(ArticlesPage), typeof(ArticlesPage));
        }
    }
}
