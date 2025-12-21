using Ecommerce.API.Data;
using Ecommerce.API.Models;
using Microsoft.EntityFrameworkCore;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, IConfiguration configuration)
    {
        // Apply pending migrations
        await context.Database.MigrateAsync();

        // Check if any users exist
        if (!await context.Users.AnyAsync(u => u.Username == "admin"))
        {
            context.Users.Add(new User
            {
                Username = configuration["Admin:Username"] ?? "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    configuration["Admin:Password"] ?? "Admin123!"
                ),
                Role = "Admin"
            });

            await context.SaveChangesAsync();
        }
    }
}
