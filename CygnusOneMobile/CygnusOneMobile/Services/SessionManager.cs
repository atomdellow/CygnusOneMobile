using CygnusOneMobile.Models;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace CygnusOneMobile.Services
{
    public class SessionManager
    {
        private readonly string _tokenKey = "auth_token";
        private readonly string _userIdKey = "user_id";
        private readonly string _userNameKey = "user_name";
        private readonly string _userEmailKey = "user_email";
        private readonly DebugLogger? _logger;
        
        // Update IsLoggedIn to check both token and user ID
        public bool IsLoggedIn 
        {
            get 
            {
                var hasUserId = Preferences.Default.ContainsKey(_userIdKey);
                var hasToken = SecureStorage.Default.GetAsync(_tokenKey).Result != null;
                
                _logger?.Log($"Session validation - HasUserId: {hasUserId}, HasToken: {hasToken}", LogLevel.Debug);
                return hasUserId && hasToken;
            }
        }
        
        public SessionManager(DebugLogger? logger = null)
        {
            _logger = logger;
            _logger?.Log("SessionManager initialized", LogLevel.Info);
        }
        
        public async Task SetSessionAsync(string token, User user)
        {
            if (string.IsNullOrEmpty(token) || user == null)
            {
                _logger?.Log("SetSessionAsync failed: Token or user is null", LogLevel.Warning);
                return;
            }
            
            try
            {
                // Store the authentication token
                await SecureStorage.Default.SetAsync(_tokenKey, token);
                
                // Store user information
                Preferences.Default.Set(_userIdKey, user.Id);
                Preferences.Default.Set(_userNameKey, user.Name);
                Preferences.Default.Set(_userEmailKey, user.Email);
                
                _logger?.Log($"Session saved for user: {user.Name} (ID: {user.Id})", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Error saving session");
                throw;
            }
        }
        
        public async Task<string?> GetTokenAsync()
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync(_tokenKey);
                if (string.IsNullOrEmpty(token))
                {
                    _logger?.Log("Retrieved token is null or empty", LogLevel.Warning);
                }
                return token;
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Error retrieving token");
                return null;
            }
        }
        
        public User? GetCurrentSession()
        {
            try
            {
                if (!Preferences.Default.ContainsKey(_userIdKey))
                {
                    _logger?.Log("No active session found", LogLevel.Info);
                    return null;
                }
                    
                var user = new User
                {
                    Id = Preferences.Default.Get(_userIdKey, 0),
                    Name = Preferences.Default.Get(_userNameKey, string.Empty),
                    Email = Preferences.Default.Get(_userEmailKey, string.Empty)
                };
                
                _logger?.Log($"Current session retrieved for user: {user.Name}", LogLevel.Debug);
                return user;
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Error retrieving user data");
                return null;
            }
        }
        
        public async Task ClearSessionAsync()
        {
            try
            {
                // Remove secure storage immediately rather than using Task.Run
                SecureStorage.Default.Remove(_tokenKey);
                Preferences.Default.Remove(_userIdKey);
                Preferences.Default.Remove(_userNameKey);
                Preferences.Default.Remove(_userEmailKey);
                
                _logger?.Log("Session cleared", LogLevel.Warning);
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Error clearing session");
                throw;
            }
        }
        
        // Add method to forcefully reset the session state for testing
        public async Task ForceResetSessionAsync()
        {
            try
            {
                // Use await with the ClearSessionAsync method
                await ClearSessionAsync();
                
                // Force the clear by setting null values
                await Task.Run(() => {
                    SecureStorage.Default.Remove(_tokenKey);
                    Preferences.Default.Clear();
                });
                
                _logger?.Log("Session forcefully reset", LogLevel.Warning);
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Error resetting session");
                throw;
            }
        }
    }
}