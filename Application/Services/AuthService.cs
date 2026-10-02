using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Application.DTO.Requests.Auth;
using Application.DTO.Responses;
using Application.Interfaces;
using Domain.Interfaces;
using Domain.Models;

namespace Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly ITokenService _jwtService;
        private readonly IHashService _hashService;

        public AuthService(
            UserManager<User> userManager,
            IRefreshTokensRepository refreshTokens,
            ITokenService jwtService,
            IHashService hashService)
        {
            _userManager = userManager;
            _refreshTokensRepository = refreshTokens;
            _jwtService = jwtService;
            _hashService = hashService;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid email or password");
            }

            var isValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!isValid)
            {
                throw new InvalidOperationException("Invalid email or password");
            }

            var accessToken = await _jwtService.GenerateAccessTokenAsync(user);

            var refreshTokenEntity = await _jwtService.GenerateRefreshTokenAsync(user);
            var rawRefreshToken = refreshTokenEntity.Token;
            refreshTokenEntity.Token = _hashService.ComputeHash(refreshTokenEntity.Token);

            await _refreshTokensRepository.AddRefreshTokenAsync(refreshTokenEntity);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                User = new UserDataResponse
                {
                    Id = user.Id.ToString(),
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email!
                }
            };
        }

        public async Task<RefreshResponse> RefreshAccessTokenAsync(RefreshRequest request)
        {
            var hashedToken = _hashService.ComputeHash(request.RefreshToken);
            var tokenEntity = await _refreshTokensRepository.GetRefreshTokenAsync(hashedToken);
            if (tokenEntity == null)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            if (!tokenEntity.IsActive)
            {
                if (tokenEntity.Revoked == null)
                {
                    tokenEntity.Revoked = DateTime.UtcNow;
                    await _refreshTokensRepository.UpdateAsync(tokenEntity);
                }
                throw new SecurityTokenException("Refresh token has expired");
            }

            if (tokenEntity.Expires < DateTime.UtcNow.AddHours(12))
            {
                tokenEntity.Expires = DateTime.UtcNow.AddDays(7);
                await _refreshTokensRepository.UpdateAsync(tokenEntity);
            }

            var user = await _userManager.FindByIdAsync(tokenEntity.UserId.ToString());
            if (user == null)
            {
                throw new SecurityTokenException("User not found");
            }

            var newAccessToken = await _jwtService.GenerateAccessTokenAsync(user);

            return new RefreshResponse
            {
                AccessToken = newAccessToken,
                User = new UserDataResponse
                {
                    Id = user.Id.ToString(),
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email!
                }
            };
        }

        public async Task LogoutAsync(LogoutRequest request)
        {
            var hashedToken = _hashService.ComputeHash(request.RefreshToken);
            var tokenEntity = await _refreshTokensRepository.GetRefreshTokenAsync(hashedToken);
            if (tokenEntity == null || !tokenEntity.IsActive)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            await _refreshTokensRepository.RevokeTokenAsync(tokenEntity);
        }

        public async Task<User> RegisterAsync(RegisterRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user != null)
            {
                throw new InvalidOperationException("Email is already in use");
            }

            var userRecord = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Surname = request.Surname ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                LastUpdate = DateTime.UtcNow,
                Email = request.Email,
                NormalizedEmail = request.Email.ToUpper(),
                UserName = $"{request.Name + request.Surname}",
                NormalizedUserName = request.Name.ToUpper(),
                EmailConfirmed = false,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(userRecord, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User creation failed: {errors}");
            }

            await _userManager.AddToRoleAsync(userRecord, "User");
            //verify email logic can be added here
            return userRecord;
        }

    }
}