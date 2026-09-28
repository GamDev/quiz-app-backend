
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizApp.Backend.Auth.Dtos;
using QuizApp.Backend.Common;
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
        private readonly ITransactionManager _transactionManager;
        private static readonly string _dummyPasswordHash = new PasswordHasher<User>().HashPassword(new User(), "dummy-password");
        public AuthService(IUserService userService,
                           ITokenService tokenService,
                           IRefreshTokenService refreshTokenService,
                           IPasswordHasher<User> passwordHasher,
                           ILogger<AuthService> logger,
                           ITransactionManager transactionManager)
        {
            _userService = userService;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _transactionManager = transactionManager;
        }

        /// <summary>
        /// Authenticates a user and issues access and refresh tokens.
        /// </summary>
        public async Task<AuthResult> AuthenticateAsync(LoginRequest loginRequest,
                                                        CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("Login attempt received");

            var user = await _userService.GetByEmailAsync(loginRequest.Email, cancellationToken);

            if (user == null)
            {
                // Perform password verification even when the user
                // does not exist. This helps reduce timing differences
                // between "unknown email" and "wrong password".
                _passwordHasher.VerifyHashedPassword(new User(), _dummyPasswordHash, loginRequest.Password);
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
            return AuthResult.Success(accessToken, refreshToken.Token, refreshToken.Expires);
        }

        /// <summary>
        /// Registers a new user and issues access and refresh tokens.
        /// </summary>
        public async Task<AuthResult> RegisterAsync(RegisterRequest registerRequest,
                                                    CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Registration attempt recieved");

            var existingUser = await _userService.GetByEmailAsync(registerRequest.Email, cancellationToken);
            if (existingUser != null)
            {
               _logger.LogWarning("Registration failed: email already taken");
                return AuthResult.Failure("Email already taken");
            }

            var user = new User
            {
                FullName = registerRequest.FullName,
                Email = registerRequest.Email,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, registerRequest.Password);


            await _transactionManager.BeginAsync(cancellationToken);

            try
            {
                await _userService.CreateUser(user, cancellationToken);

                var accessToken = _tokenService.GenerateAccessToken(user);

                var refreshToken = await _refreshTokenService.CreateAsync(user, cancellationToken);

                await _transactionManager.CommitAsync(cancellationToken);

                _logger.LogInformation("User registered successfully: {UserId}", user.Id);

                return AuthResult.Success(accessToken, refreshToken.Token, refreshToken.Expires);
            }
            catch (DbUpdateException)
            {
                // Two people tried to register with the same email at the same time.
                // The first one was saved. The database rejected the second one.
                // So we return "Email already taken" instead of crashing.
                await _transactionManager.RollbackAsync(cancellationToken);
                _logger.LogWarning("Registration failed: email already taken (concurrent request)");
                return AuthResult.Failure("Email already taken");
            }
            catch
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                throw;
            }
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

            if (refreshToken == null)
            {
                _logger.LogWarning("Unknown refresh token used");
                return null;
            }

            // Token was already replaced by a newer one: someone is reusing an old token.
            if (refreshToken.Revoked != null && refreshToken.ReplacedByToken != null)
            {
                // Grace period: two tabs can refresh at almost the same time.
                var justRotated = refreshToken.Revoked > DateTime.UtcNow.AddSeconds(-10);

                if (!justRotated)
                {
                    _logger.LogWarning("Refresh token reuse detected for user {UserId}. Revoking all tokens.",
                                       refreshToken.UserId);
                    await _refreshTokenService.RevokeAllForUserAsync(refreshToken.UserId, cancellationToken);
                }

                return null;
            }

            if (!refreshToken.IsActive)
            {
                _logger.LogWarning("Invalid or expired refresh token used");
                return null;
            }

            var newRefreshToken = await _refreshTokenService.RotateAsync(refreshToken, cancellationToken);

            if (newRefreshToken == null)
            {
                _logger.LogWarning("Refresh token was already used by a concurrent request");
                return null;
            }

            var accessToken = _tokenService.GenerateAccessToken(refreshToken.User);

            _logger.LogInformation("Refresh token rotated successfully for user {UserId}", refreshToken.UserId);
            return AuthResult.Success(accessToken, newRefreshToken.Token, newRefreshToken.Expires);
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
