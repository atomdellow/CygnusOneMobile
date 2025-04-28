using System.Text.Json.Serialization;

namespace CygnusOneMobile.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }
        
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
        
        public Dictionary<string, object> Permissions { get; set; }
        
        // Helper method to check if the user has a specific permission
        public bool HasPermission(string permissionPath)
        {
            // Admin always has all permissions
            if (Role == "admin")
                return true;
                
            if (Permissions == null)
                return false;
                
            // Navigate through permission path
            string[] parts = permissionPath.Split('.');
            
            var currentLevel = Permissions;
            foreach (var part in parts)
            {
                if (currentLevel.TryGetValue(part, out var value))
                {
                    if (value is Dictionary<string, object> dict)
                    {
                        currentLevel = dict;
                    }
                    else if (value is bool boolValue)
                    {
                        return boolValue;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            
            return false;
        }
        
        public bool IsAdmin => Role == "admin";
        public bool IsEditor => Role == "admin" || Role == "editor";
    }
}