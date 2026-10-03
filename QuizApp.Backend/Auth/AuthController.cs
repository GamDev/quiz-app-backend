using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizApp.Backend.Auth.Dtos;
using QuizApp.Backend.Common;
using QuizApp.Backend.Tokens;
using QuizApp.Backend.Tokens.Dtos;
using QuizApp.Backend.Users;
using QuizApp.Backend.Users.Dtos;

namespace QuizApp.Backend.Auth
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private const string RefreshTokenCookieName = "refreshToken";
        private const string AuthCookiePath = "/api/auth";
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IWebHostEnvironment _environment;
        private readonly IUserService _userService;
        public AuthController(IAuthService authService,
                              ITokenService tokenService,
                              IWebHostEnvironment environment,
                              IUserService userService)
        {
            _authService = authService;
            _tokenService = tokenService;
            _environment = environment;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request,
                                                  CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new ApiResponse<object>(false, null, result.Error));

            SetRefreshTokenCookie(result.RefreshToken!, result.RefreshTokenExpiresAt!.Value);

            return Ok(new ApiResponse<AuthResponse>(
                true,
                new AuthResponse(result.AccessToken!, _tokenService.AccessTokenExpiryInSeconds)));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request,
                                               CancellationToken cancellationToken)
        {
            var result = await _authService.AuthenticateAsync(request, cancellationToken);

            if (!result.IsSuccess)
                return Unauthorized(new ApiResponse<AuthResponse>(false, null, result.Error));

            SetRefreshTokenCookie(result.RefreshToken!, result.RefreshTokenExpiresAt!.Value);

            return Ok(new ApiResponse<AuthResponse>(true,
                     new AuthResponse(result.AccessToken!,
                      _tokenService.AccessTokenExpiryInSeconds)));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized(new ApiResponse<AuthResponse>(
                        false, null, "Refresh token cookie is missing"));
            }

            var result = await _authService.RefreshTokenAsync(refreshToken, cancellationToken);

            if (!result.IsSuccess)
            {
                return Unauthorized(new ApiResponse<AuthResponse>(false, null, "Invalid or expired refresh token"));
            }

            SetRefreshTokenCookie(result.RefreshToken!, result.RefreshTokenExpiresAt!.Value);

            return Ok(new ApiResponse<AuthResponse>(true,
                      new AuthResponse(result.AccessToken!,
                                        _tokenService.AccessTokenExpiryInSeconds)));
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken(CancellationToken cancellationToken)
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Ok(new ApiResponse<object>(false, null, "Already logged out"));
            }
            var revoked = await _authService.RevokeTokenAsync(refreshToken, cancellationToken);

            DeleteRefreshTokenCookie();

            if (!revoked)
                return Ok(new ApiResponse<object>(true, null, "Already logged out"));

            return Ok(new ApiResponse<object>(true, null, "Refresh token revoked successfully"));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new ApiResponse<object>(false, null, "Invalid token"));

            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);

            if (user is null)
                return Unauthorized(new ApiResponse<object>(false, null, "User not found"));

            return Ok(new ApiResponse<UserResponse>(true, user));
        }
        private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAt)
        {
            Response.Cookies.Append(RefreshTokenCookieName, refreshToken,
                            new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = !_environment.IsDevelopment(),
                                SameSite = SameSiteMode.Strict,
                                Path = AuthCookiePath,
                                Expires = DateTime.SpecifyKind(expiresAt, DateTimeKind.Utc)
                            });
        }
        private void DeleteRefreshTokenCookie()
        {
            Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Path = AuthCookiePath
            });
        }

    }
}
