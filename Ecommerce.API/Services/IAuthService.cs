using Ecommerce.API.DTOs;

namespace Ecommerce.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(string username, string password);
        Task RegisterAsync(string username, string password);
        Task<string> RefreshTokenAsync(string refreshToken);
    }
}
