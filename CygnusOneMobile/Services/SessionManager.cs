using CygnusOneMobile.Models;

namespace CygnusOneMobile.Services
{
    public class SessionManager
    {
        private const string TokenKey = "auth_token";
        private const string UserKey = "current_user";
        
        private readonly ISecureStorage _secureStorage;
        
        public SessionManager()
        {
            _secureStorage = SecureStorage.Default;
        }
        
        public async Task SaveTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                await _secureStorage.SetAsync(TokenKey, string.Empty);
            }
            else
            {
                await _secureStorage.SetAsync(TokenKey, token);
            }
        }
        
        public async Task<string> GetTokenAsync()
        {
            return await _secureStorage.GetAsync(TokenKey);
        }
        
        public async Task RemoveTokenAsync()
        {
            SecureStorage.Remove(TokenKey);
        }
        
        public async Task SaveUserAsync(User user)
        {
            if (user == null)
            {
                Preferences.Remove(UserKey);
            }
            else
            {
                string userJson = System.Text.Json.JsonSerializer.Serialize(user);
                Preferences.Set(UserKey, userJson);
            }
        }
        
        public User GetUser()
        {
            string userJson = Preferences.Get(UserKey, null);
            
            if (string.IsNullOrEmpty(userJson))
                return null;
                
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<User>(userJson);
            }
            catch
            {
                return null;
            }
        }
        
        public void RemoveUser()
        {
            Preferences.Remove(UserKey);
        }
        
        public async Task ClearSessionAsync()
        {
            await RemoveTokenAsync();
            RemoveUser();
        }
        
        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token);
        }
    }
}