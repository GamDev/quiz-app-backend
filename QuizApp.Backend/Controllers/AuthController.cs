using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using com.QuizAppBackend.Dtos;
using com.QuizAppBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace com.QuizAppBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request,
                                                  CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new ApiResponse<object>(false, null, result.Error));

            return Ok(new ApiResponse<AuthResponse>(
                true,
                new AuthResponse(result.AccessToken!, result.RefreshToken!, 3600)));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request,
                                               CancellationToken cancellationToken)
        {
            var result = await _authService.AuthenticateAsync(request, cancellationToken);

            if (!result.IsSuccess)
                return Unauthorized(new ApiResponse<AuthResponse>(false, null, result.Error));

            return Ok(new ApiResponse<AuthResponse>(
                true,
                new AuthResponse(result.AccessToken!, result.RefreshToken!, 3600)));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request,
                                                       CancellationToken cancellationToken)
        {
            var result = await _authService.RefreshTokenAsync(request.Token, cancellationToken);

            if (result == null)
                return Unauthorized(new ApiResponse<AuthResponse>(false, null, "Invalid or expired refresh token"));

            return Ok(new ApiResponse<AuthResponse>(
                true,
                new AuthResponse(result.AccessToken!, result.RefreshToken!, 3600)));
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request,
                                                     CancellationToken cancellationToken)
        {
            var revoked = await _authService.RevokeTokenAsync(request.Token, cancellationToken);

            if (!revoked)
                return NotFound(new ApiResponse<RevokeTokenRequest>(false, null, "Token not found or already revoked"));

            return Ok(new ApiResponse<RevokeTokenRequest>(true, null, "Refresh token revoked successfully"));
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var email = User.FindFirstValue(ClaimTypes.Email)
                      ?? User.FindFirstValue(JwtRegisteredClaimNames.Email);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
                return Unauthorized(new ApiResponse<object>(false, null, "Invalid token"));

            return Ok(new ApiResponse<MeResponse>(true, new MeResponse(userId, email)));
        }
    }
}
