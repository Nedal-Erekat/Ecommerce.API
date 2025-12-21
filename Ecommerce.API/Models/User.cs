namespace Ecommerce.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // store hashed password
        public string Role { get; set; } = "Customer"; // "Admin" or "Customer"
    }
}
