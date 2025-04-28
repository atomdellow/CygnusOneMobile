using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CygnusOneMobile.Models;

namespace CygnusOneMobile.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseApiUrl;
        private readonly JsonSerializerOptions _jsonOptions;
        
        public AuthService()
        {
            _httpClient = new HttpClient();
            _baseApiUrl = DeviceInfo.Platform == DevicePlatform.Android 
                ? "http://10.0.2.2:5000/api" // Use 10.0.2.2 for Android emulator to reach localhost
                : "http://localhost:5000/api";
                
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }
        
        public async Task<(bool success, string token, User user, string errorMessage)> LoginAsync(string email, string password)
        {
            try
            {
                var loginData = new
                {
                    email,
                    password
                };
                
                var content = new StringContent(
                    JsonSerializer.Serialize(loginData), 
                    Encoding.UTF8, 
                    "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseApiUrl}/auth/login", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<LoginResponse>(jsonResponse, _jsonOptions);
                    
                    return (true, result.Token, result.User, null);
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    var error = JsonSerializer.Deserialize<ErrorResponse>(errorResponse, _jsonOptions);
                    return (false, null, null, error?.Message ?? "Login failed");
                }
            }
            catch (Exception ex)
            {
                return (false, null, null, $"Connection error: {ex.Message}");
            }
        }
        
        public async Task<(bool success, User user, string errorMessage)> GetCurrentUserAsync(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync($"{_baseApiUrl}/auth/me");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<UserResponse>(jsonResponse, _jsonOptions);
                    
                    return (true, result.User, null);
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    var error = JsonSerializer.Deserialize<ErrorResponse>(errorResponse, _jsonOptions);
                    return (false, null, error?.Message ?? "Failed to retrieve user data");
                }
            }
            catch (Exception ex)
            {
                return (false, null, $"Connection error: {ex.Message}");
            }
        }
        
        public async Task<bool> LogoutAsync(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.PostAsync($"{_baseApiUrl}/auth/logout", null);
                
                return response.IsSuccessStatusCode;
            }
            catch
            {
                // Even if logout fails on server, we'll clear local session anyway
                return true;
            }
        }
        
        private class LoginResponse
        {
            public string Token { get; set; }
            public User User { get; set; }
        }
        
        private class UserResponse
        {
            public User User { get; set; }
        }
        
        private class ErrorResponse
        {
            public string Message { get; set; }
        }
    }
}