using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CygnusOneMobile.Models;
using CygnusOneMobile.Views;
using CommunityToolkit.Mvvm.Messaging;

namespace CygnusOneMobile.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly DebugLogger? _logger;
        private readonly SessionManager _sessionManager;
        
        public string ApiBaseUrl { get; set; }
        
        public AuthService(SessionManager sessionManager, DebugLogger? logger = null)
        {
            _logger = logger;
            _sessionManager = sessionManager;
            
            // Use the Heroku staging API URL
            ApiBaseUrl = "https://cygnusone-staging-572cc33c3856.herokuapp.com/api";
            
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30) // Increased timeout for remote API
            };
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Ensure property names are camelCase in JSON
            };
            
            _logger?.Log($"AuthService initialized with URL: {ApiBaseUrl}", LogLevel.Info);
            
            // Test the connection immediately
            Task.Run(async () => {
                var isConnected = await TestConnection();
                _logger?.Log($"Initial connection test: {(isConnected ? "Success" : "Failed")}", LogLevel.Info);
            });
        }
        
        public async Task<(bool success, string? token, User? user, string? errorMessage)> LoginAsync(string email, string password)
        {
            try
            {
                _logger?.Log($"Attempting login to {ApiBaseUrl}/auth/login", LogLevel.Info);
                
                // Make sure URL is properly formatted
                var loginUrl = $"{ApiBaseUrl}/auth/login";
                if (!loginUrl.StartsWith("http"))
                {
                    loginUrl = "https://" + loginUrl;
                }
                
                // Create login payload
                var loginData = new
                {
                    email = email.Trim(), // ensure trimmed values and use camelCase property names
                    password = password
                };
                
                // Serialize with proper options
                var json = JsonSerializer.Serialize(loginData, _jsonOptions);
                _logger?.Log($"Login payload: {json}", LogLevel.Debug);
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Log raw request for debugging
                _logger?.Log($"Making POST request to {loginUrl} with content type {content.Headers.ContentType}", LogLevel.Debug);
                
                // Try directly using HttpClient
                var response = await _httpClient.PostAsync(loginUrl, content);
                
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger?.Log($"Login response code: {response.StatusCode}", LogLevel.Debug);
                _logger?.Log($"Login response body: {TruncateString(responseBody, 500)}", LogLevel.Debug);
                
                if (response.IsSuccessStatusCode)
                {
                    try {
                        var jsonDocument = JsonNode.Parse(responseBody);
                        if (jsonDocument != null)
                        {
                            var tokenNode = jsonDocument["token"];
                            var userNode = jsonDocument["user"];
                            
                            if (tokenNode != null && userNode != null)
                            {
                                var token = tokenNode.GetValue<string>();
                                var user = new User
                                {
                                    Id = userNode["id"]?.GetValue<int>() ?? 0,
                                    Email = userNode["email"]?.GetValue<string>() ?? string.Empty,
                                    Name = userNode["name"]?.GetValue<string>() ?? string.Empty
                                };
                                
                                _logger?.Log($"Login successful for user: {user.Email}", LogLevel.Info);
                                
                                // Save session and broadcast login event
                                await _sessionManager.SetSessionAsync(token, user);
                                // Send message using WeakReferenceMessenger
                                WeakReferenceMessenger.Default.Send("UserLoggedIn");
                                
                                return (true, token, user, null);
                            }
                        }
                    }
                    catch (JsonException jex) {
                        _logger?.LogException(jex, "JSON parsing error in login response");
                        return (false, null, null, "Error parsing server response");
                    }
                    
                    _logger?.Log("Invalid response format from server", LogLevel.Error);
                    return (false, null, null, "Invalid response format from server");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger?.Log("Login failed: Invalid credentials", LogLevel.Warning);
                    return (false, null, null, "Invalid credentials");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    _logger?.Log($"Bad request response: {responseBody}", LogLevel.Error);
                    return (false, null, null, "Invalid email or password format");
                }
                else
                {
                    _logger?.Log($"Server error: {response.StatusCode}", LogLevel.Error);
                    return (false, null, null, $"Server error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Login exception");
                return (false, null, null, $"Connection error: {ex.Message}");
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                _logger?.Log("Logging out user", LogLevel.Info);
                
                // Clear session data
                await _sessionManager.ClearSessionAsync();
                
                // Broadcast logout event using WeakReferenceMessenger
                WeakReferenceMessenger.Default.Send("UserLoggedOut");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Logout exception");
                return false;
            }
        }
        
        public async Task<bool> TestConnection()
        {
            try
            {
                _logger?.Log($"Testing connection to {ApiBaseUrl}", LogLevel.Info);
                
                var testUrl = ApiBaseUrl;
                if (!testUrl.StartsWith("http"))
                {
                    testUrl = "https://" + testUrl;
                }
                
                var response = await _httpClient.GetAsync(testUrl);
                var responseBody = await response.Content.ReadAsStringAsync();
                
                _logger?.Log($"Connection test response: {response.StatusCode}, Body: {TruncateString(responseBody, 100)}", LogLevel.Debug);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Connection test failed");
                return false;
            }
        }
        
        private string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
                return input;
                
            return input.Substring(0, maxLength) + "...";
        }

        public async Task<(bool success, string? token, User? user, string? errorMessage)> RegisterAsync(string email, string password)
        {
            try
            {
                _logger?.Log($"Attempting registration to {ApiBaseUrl}/auth/register", LogLevel.Info);
                
                // Make sure URL is properly formatted
                var registerUrl = $"{ApiBaseUrl}/auth/register";
                if (!registerUrl.StartsWith("http"))
                {
                    registerUrl = "https://" + registerUrl;
                }
                
                // Safety check for the email to prevent index errors
                string username = "user";
                if (!string.IsNullOrEmpty(email) && email.Contains("@"))
                {
                    var parts = email.Split('@');
                    if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
                    {
                        username = parts[0];
                    }
                }
                
                // Create registration payload using camelCase property names
                var registerData = new
                {
                    email = email.Trim(), // ensure trimmed values
                    password = password,
                    name = username // Safely extracted username
                };
                
                // Serialize with proper options
                var json = JsonSerializer.Serialize(registerData, _jsonOptions);
                _logger?.Log($"Registration payload: {json}", LogLevel.Debug);
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Log raw request for debugging
                _logger?.Log($"Making POST request to {registerUrl} with content type {content.Headers.ContentType}", LogLevel.Debug);
                
                // Try directly using HttpClient
                var response = await _httpClient.PostAsync(registerUrl, content);
                
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger?.Log($"Registration response code: {response.StatusCode}", LogLevel.Debug);
                _logger?.Log($"Registration response body: {TruncateString(responseBody, 500)}", LogLevel.Debug);
                
                if (response.IsSuccessStatusCode)
                {
                    try {
                        var jsonDocument = JsonNode.Parse(responseBody);
                        if (jsonDocument != null)
                        {
                            var tokenNode = jsonDocument["token"];
                            var userNode = jsonDocument["user"];
                            
                            if (tokenNode != null && userNode != null)
                            {
                                var token = tokenNode.GetValue<string>();
                                var user = new User
                                {
                                    Id = userNode["id"]?.GetValue<int>() ?? 0,
                                    Email = userNode["email"]?.GetValue<string>() ?? string.Empty,
                                    Name = userNode["name"]?.GetValue<string>() ?? string.Empty
                                };
                                
                                _logger?.Log($"Registration successful for user: {user.Email}", LogLevel.Info);
                                
                                // Save session and broadcast login event
                                await _sessionManager.SetSessionAsync(token, user);
                                WeakReferenceMessenger.Default.Send("UserLoggedIn");
                                
                                return (true, token, user, null);
                            }
                        }
                    }
                    catch (JsonException jex) {
                        _logger?.LogException(jex, "JSON parsing error in registration response");
                        return (false, null, null, "Error parsing server response");
                    }
                    
                    _logger?.Log("Invalid response format from server", LogLevel.Error);
                    return (false, null, null, "Invalid response format from server");
                }
                else if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    _logger?.Log("Registration failed: Email already exists", LogLevel.Warning);
                    return (false, null, null, "Email address already registered");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    _logger?.Log($"Bad request response: {responseBody}", LogLevel.Error);
                    return (false, null, null, "Invalid email or password format");
                }
                else
                {
                    _logger?.Log($"Server error: {response.StatusCode}", LogLevel.Error);
                    return (false, null, null, $"Server error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Registration exception");
                return (false, null, null, $"Connection error: {ex.Message}");
            }
        }
    }
}