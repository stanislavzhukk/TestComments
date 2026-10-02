using Microsoft.AspNetCore.Mvc;
using Services.Services;
using Application.DTO.Requests.Auth;
using Application.DTO.Responses;
using Application.Interfaces;

namespace API.Controllers
{
    [Controller]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(new { message = "Registration successful." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for email: {Email}", request.Email);
                return StatusCode(500, new { message = $"An error occurred during registration.", details = $"{ex.Message}" });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                _logger.LogInformation($"User: {response.User}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for email: {Email}", request.Email);
                return Unauthorized(new { message = "Invalid email or password." });
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<RefreshResponse>> Refresh([FromBody] RefreshRequest request)
        {
            try
            {
                var response = await _authService.RefreshAccessTokenAsync(request);
                if (response == null)
                {
                    return Unauthorized();
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed for refresh token: {RefreshToken}", request.RefreshToken);
                return Unauthorized(new { message = ex.Message });

            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                await _authService.LogoutAsync(request);
                return Ok(new { message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed for refresh token: {RefreshToken}", request.RefreshToken);
                return StatusCode(500, new { message = "An error occurred during logout.", details = $"{ex.Message}" });
            }
        }
    }
}
