namespace Ecommerce.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // store hashed password
        public string Role { get; set; } = "Customer"; // "Admin" or "Customer"

        // 🔐 Refresh token
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }

        // 🔒 Locking (used later)
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }
}
