using Application.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Identity
{
    public class JwtService : ITokenService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public JwtService(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<string> GenerateAccessTokenAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var secret = _configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey not configured");

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationInMinutes");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                SigningCredentials = credentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(securityToken);
        }

        public Task<RefreshToken> GenerateRefreshTokenAsync(User user)
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshTokenString = Convert.ToBase64String(tokenBytes);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshTokenString,
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow
            };

            return Task.FromResult(refreshToken);
        }
    }
}
