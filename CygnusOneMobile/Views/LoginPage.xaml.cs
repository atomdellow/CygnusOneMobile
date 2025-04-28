using CygnusOneMobile.ViewModels;
using CygnusOneMobile.Services;

namespace CygnusOneMobile.Views
{
    public partial class LoginPage : ContentPage
    {
        private AuthViewModel _viewModel => BindingContext as AuthViewModel;

        public LoginPage(AuthViewModel viewModel, AuthService authService, SessionManager sessionManager, DebugLogger logger = null)
        {
            InitializeComponent();
            
            // Set the ViewModel directly - fixes binding issue
            if (viewModel == null)
            {
                viewModel = new AuthViewModel(authService, sessionManager, logger);
            }
            
            BindingContext = viewModel;
            
            // Add debug helpers for button visibility
            EmailEntry.Placeholder = "Email (required)";
            PasswordEntry.Placeholder = "Password (required)";
            
            // Force the UI to update bindings
            OnPropertyChanged(nameof(BindingContext));
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // This ensures the viewmodel's validation logic is run when text changes
            // Commands will be enabled/disabled automatically based on validation
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Add visual feedback that button was clicked
            LoginButton.Text = "Logging in...";
            
            if (_viewModel != null && _viewModel.LoginCommand.CanExecute(null))
            {
                await _viewModel.LoginCommand.ExecuteAsync(null);
            }
            
            // Restore button text
            LoginButton.Text = "Login";
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            // Add visual feedback that button was clicked
            RegisterButton.Text = "Registering...";
            
            if (_viewModel != null && _viewModel.RegisterCommand.CanExecute(null))
            {
                await _viewModel.RegisterCommand.ExecuteAsync(null);
            }
            
            // Restore button text
            RegisterButton.Text = "Register";
        }

        private void OnTogglePasswordClicked(object sender, EventArgs e)
        {
            // Direct UI manipulation instead of going through ViewModel
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
            TogglePasswordButton.Text = PasswordEntry.IsPassword ? "👁️" : "🔒";
            
            // Still update the viewmodel state to stay in sync
            if (_viewModel != null)
            {
                _viewModel.IsPasswordHidden = PasswordEntry.IsPassword;
            }
        }

        private async void OnDebugClicked(object sender, EventArgs e)
        {
            // Add visual feedback
            DebugButton.BackgroundColor = Colors.DarkGray;
            
            try
            {
                if (_viewModel != null && _viewModel.OpenDebugPageCommand.CanExecute(null))
                {
                    await _viewModel.OpenDebugPageCommand.ExecuteAsync(null);
                }
                else
                {
                    // Direct navigation as fallback
                    await Shell.Current.GoToAsync("DebugRoute");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug navigation error: {ex.Message}");
                // Try direct navigation as another fallback
                try
                {
                    await Shell.Current.GoToAsync("//DebugPage");
                }
                catch
                {
                    // Last resort - show an alert
                    await DisplayAlert("Debug", "Could not navigate to debug page", "OK");
                }
            }
            finally
            {
                // Restore button appearance
                DebugButton.BackgroundColor = Color.Parse("#6c757d");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Force refresh any bindings
            if (_viewModel != null)
            {
                // This ensures bindings are refreshed
                OnPropertyChanged(nameof(BindingContext));
            }
        }
    }
}