using Ecommerce.API.Data;
using Ecommerce.API.DTOs;
using Ecommerce.API.Models;
using Ecommerce.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

    public async Task<AuthResponseDto> LoginAsync(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid username or password");

        if (user.LockoutEnd > DateTime.UtcNow)
            throw new UnauthorizedAccessException("Account locked. Try again later.");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginAttempts = 0;
            }

            await _context.SaveChangesAsync();
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // ✅ Successful login → reset
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;

        var accessToken = GenerateJwtToken(user);
        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = user.RefreshToken,
            ExpiresIn = int.Parse(_config["Jwt:ExpireMinutes"]!) * 60
        };
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

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public async Task<string> RefreshTokenAsync(string refreshToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.RefreshToken == refreshToken &&
            u.RefreshTokenExpiresAt > DateTime.UtcNow);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        var newAccessToken = GenerateJwtToken(user);

        return newAccessToken;
    }

}
