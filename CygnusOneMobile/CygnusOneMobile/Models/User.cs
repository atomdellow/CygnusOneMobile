namespace CygnusOneMobile.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserId => Id.ToString(); // Property alias for Id as string
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty; // Authentication token
    }
}