
using Microsoft.AspNetCore.Identity;

using QuizApp.Backend.Auth.Dtos;
using QuizApp.Backend.Tokens;
using QuizApp.Backend.Users;

namespace QuizApp.Backend.Auth
{

    /// <summary>
    /// Provides authentication operations including user registration,
    /// login, refresh token rotation, and refresh token revocation.
    /// </summary>
    public sealed class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserService userService,
                           ITokenService tokenService,
                           IRefreshTokenService refreshTokenService,
                           IPasswordHasher<User> passwordHasher,
                           ILogger<AuthService> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates a user and issues access and refresh tokens.
        /// </summary>
        public async Task<AuthResult> AuthenticateAsync(LoginRequest loginRequest,
                                                        CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("Login attempt for email {Email}", loginRequest.Email);

            var user = await _userService.GetByEmailAsync(loginRequest.Email, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Login failed: invalid credentials");
                return AuthResult.Failure("Invalid credentials");
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginRequest.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Login failed: invalid credentials for user {UserId}", user.Id);
                return AuthResult.Failure("Invalid credentials");
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = await _refreshTokenService.CreateAsync(user, cancellationToken);

            _logger.LogInformation("Login successful for user {UserId}", user.Id);
            return AuthResult.Success(accessToken, refreshToken.Token);
        }

        /// <summary>
        /// Registers a new user and issues access and refresh tokens.
        /// </summary>
        public async Task<AuthResult> RegisterAsync(RegisterRequest registerRequest,
                                                    CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("Registration attempt for email {Email}", registerRequest.Email);

            var existingUser = await _userService.GetByEmailAsync(registerRequest.Email, cancellationToken);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: email {Email} already taken", registerRequest.Email);
                return AuthResult.Failure("Email already taken");
            }

            var user = new User
            {
                FullName = registerRequest.FullName,
                Email = registerRequest.Email,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, registerRequest.Password);

            await _userService.CreateUser(user, cancellationToken);

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = await _refreshTokenService.CreateAsync(user, cancellationToken);

            _logger.LogInformation("User registered successfully: {UserId}", user.Id);
            return AuthResult.Success(accessToken, refreshToken.Token);
        }

        /// <summary>
        /// Validates a refresh token, revokes it,
        /// and issues a replacement access and refresh token.
        /// </summary>
        public async Task<AuthResult?> RefreshTokenAsync(string token,
                                                         CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var refreshToken = await _refreshTokenService.GetRefreshTokenWithUserAsync(token, cancellationToken);

            if (refreshToken == null || !refreshToken.IsActive)
            {
                _logger.LogWarning("Invalid or expired refresh token used");
                return null;
            }
            
            var newRefreshToken = await _refreshTokenService.RotateAsync(refreshToken, cancellationToken);
            refreshToken.ReplacedByToken = newRefreshToken.Token;

            var accessToken = _tokenService.GenerateAccessToken(refreshToken.User);

            _logger.LogInformation("Refresh token rotated successfully for user {UserId}", refreshToken.User.Id);
            return AuthResult.Success(accessToken, newRefreshToken.Token);
        }
    

        /// <summary>
        /// Revokes a refresh token so it can no longer be used.
        /// </summary>
        public async Task<bool> RevokeTokenAsync(string token,
                                                 CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var success = await _refreshTokenService.RevokeAsync(token, cancellationToken);
            if (success)
                _logger.LogInformation("Refresh token revoked successfully");
            else
                _logger.LogWarning("Attempted to revoke invalid or inactive token");
            return success;
        }
    }
}
