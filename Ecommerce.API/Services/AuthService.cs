using Ecommerce.API.Data;
using Ecommerce.API.Models;
using Ecommerce.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService : IAuthService
{   
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly byte[] _jwtKey;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;

        var key = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key missing");

        _jwtKey = Encoding.UTF8.GetBytes(key);
    }

    private string GenerateJwtToken(User user)
    {

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(_jwtKey),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(_config["Jwt:ExpireMinutes"]!)
            ),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password");

        return GenerateJwtToken(user);
    }

    public async Task RegisterAsync(string username, string password)
    {
        var exists = await _context.Users
         .AnyAsync(u => u.Username == username);

        if (exists)
            throw new BadHttpRequestException("Username already exists");

        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Customer"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
}
