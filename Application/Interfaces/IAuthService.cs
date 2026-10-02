using Application.DTO.Requests.Auth;
using Application.DTO.Responses;
using Domain.Models;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task LogoutAsync(LogoutRequest request);
        Task<User> RegisterAsync(RegisterRequest request);
        Task<RefreshResponse> RefreshAccessTokenAsync(RefreshRequest request);
    }
}
