using CygnusOneMobile.ViewModels;
using CygnusOneMobile.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CygnusOneMobile.Views
{
    public partial class LoginPage : ContentPage
    {
        // Create a property to hold the ViewModel
        private AuthViewModel? _viewModel;

        public LoginPage()
        {
            InitializeComponent();
            
            try
            {
                // Create services in correct order
                var sessionManager = new SessionManager();
                var authService = new AuthService(sessionManager, null);
                
                // Create and set the ViewModel with the services
                _viewModel = new AuthViewModel(authService, sessionManager);

                // Set as binding context
                BindingContext = _viewModel;
                
                // Add direct event handlers as a fallback for the binding commands
                LoginButton.Clicked += OnLoginClicked;
                RegisterButton.Clicked += OnRegisterClicked;
                DebugButton.Clicked += OnDebugClicked;
                TogglePasswordButton.Clicked += OnTogglePasswordClicked;
            }
            catch (Exception ex)
            {
                // Handle initialization errors
                System.Diagnostics.Debug.WriteLine($"Error initializing LoginPage: {ex.Message}");
            }
        }
        
        // Direct event handler for toggling password visibility
        private void OnTogglePasswordClicked(object? sender, EventArgs e)
        {
            // Direct UI manipulation for immediate feedback
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
            TogglePasswordButton.Text = PasswordEntry.IsPassword ? "👁️" : "🔒";
            
            // Also update the viewmodel if it exists
            if (_viewModel != null && _viewModel.TogglePasswordVisibilityCommand?.CanExecute(null) == true)
            {
                _viewModel.TogglePasswordVisibilityCommand.Execute(null);
            }
        }
        
        // Direct event handlers as backup in case bindings don't work
        private async void OnLoginClicked(object? sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                try
                {
                    // Show a direct indication that the button was clicked
                    LoginButton.Text = "Logging in...";
                    await Task.Delay(100); // Ensure UI updates
                    
                    if (_viewModel.LoginCommand?.CanExecute(null) == true)
                    {
                        _viewModel.LoginCommand.Execute(null);
                    }
                    else
                    {
                        // Direct fallback if command isn't working
                        await DisplayAlert("Login", "Attempting to log in...", "OK");
                    }
                }
                catch (Exception ex)
                {
                    // Show any errors
                    await DisplayAlert("Error", $"Login error: {ex.Message}", "OK");
                }
                finally
                {
                    // Reset the button
                    LoginButton.Text = "Login";
                }
            }
            else
            {
                await DisplayAlert("Login", "Login button clicked - but ViewModel not available", "OK");
            }
        }

        private async void OnRegisterClicked(object? sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                try
                {
                    // Show a direct indication that the button was clicked
                    RegisterButton.Text = "Registering...";
                    await Task.Delay(100); // Ensure UI updates
                    
                    if (_viewModel.RegisterCommand?.CanExecute(null) == true)
                    {
                        _viewModel.RegisterCommand.Execute(null);
                    }
                    else
                    {
                        // Direct fallback if command isn't working
                        await DisplayAlert("Register", "Attempting to register...", "OK");
                    }
                }
                catch (Exception ex)
                {
                    // Show any errors
                    await DisplayAlert("Error", $"Registration error: {ex.Message}", "OK");
                }
                finally
                {
                    // Reset the button
                    RegisterButton.Text = "Register";
                }
            }
            else
            {
                await DisplayAlert("Register", "Register button clicked - but ViewModel not available", "OK");
            }
        }

        private async void OnDebugClicked(object? sender, EventArgs e)
        {
            try
            {
                // Change button color to provide feedback
                DebugButton.BackgroundColor = Colors.DarkGray;
                
                if (_viewModel?.OpenDebugPageCommand?.CanExecute(null) == true)
                {
                    _viewModel.OpenDebugPageCommand.Execute(null);
                }
                else
                {
                    // Try direct navigation as a fallback
                    try
                    {
                        await Shell.Current.GoToAsync("//DebugPage");
                    }
                    catch
                    {
                        await DisplayAlert("Debug", "Opening debug tools...", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Debug navigation error: {ex.Message}", "OK");
            }
            finally
            {
                // Reset button appearance
                DebugButton.BackgroundColor = Color.Parse("#6c757d");
            }
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Force refresh the bindings when the page appears
            OnPropertyChanged(nameof(BindingContext));
        }
    }
}