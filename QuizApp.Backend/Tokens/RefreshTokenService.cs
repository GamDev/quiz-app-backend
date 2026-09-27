

using QuizApp.Backend.Users;

namespace QuizApp.Backend.Tokens
{
    /// <summary>
    /// Manages the lifecycle of refresh tokens including creation,
    /// rotation, revocation, retrieval, and cleanup.
    /// </summary>
    public sealed class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<RefreshTokenService> _logger;
        public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository,
                                   ITokenService tokenService,
                                   ILogger<RefreshTokenService> logger)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Creates and stores a new refresh token for the specified user.
        /// </summary>
        public async Task<RefreshToken> CreateAsync(User user,
                                                    CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var newToken = _tokenService.GenerateRefreshToken();

            newToken.UserId = user.Id;

            _refreshTokenRepository.Add(newToken);

            await RemoveInactiveTokensAsync(user, cancellationToken, commit: false);

            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created refresh token for user {UserId}, expires at {Expiry}",
                                     user.Id,
                                     newToken.Expires);

            return newToken;
        }

        /// <summary>
        /// Revokes the specified refresh token and creates a replacement.
        /// </summary>
        public async Task<RefreshToken> RotateAsync(RefreshToken refreshToken,
                                                    CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!refreshToken.IsActive)
            {
                throw new InvalidOperationException("Cannot rotate an inactive refresh token.");
            }

            refreshToken.Revoked = DateTime.UtcNow;

            var newToken = _tokenService.GenerateRefreshToken();

            newToken.UserId = refreshToken.UserId;

            refreshToken.ReplacedByToken = newToken.Token;

            _refreshTokenRepository.Add(newToken);

            await RemoveInactiveTokensAsync(refreshToken.User, cancellationToken, commit: false);

            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Rotated refresh token for user {UserId}, new token expires at {Expiry}",
                                    refreshToken.UserId,
                                    newToken.Expires);

            return newToken;
        }
        /// <summary>
        /// Revokes a refresh token so it can no longer be used.
        /// </summary>
        public async Task<bool> RevokeAsync(string token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var refreshToken = await _refreshTokenRepository.GetByTokenWithUserAsync(token, cancellationToken);
            if (refreshToken == null || !refreshToken.IsActive)
            {
                _logger.LogWarning("Attempted to revoke invalid or inactive token");
                return false;
            }

            refreshToken.Revoked = DateTime.UtcNow;

            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Revoked refresh token for user {UserId}", refreshToken.User?.Id);

            return true;
        }

        /// <summary>
        /// Fetch a refresh token along with its associated user.
        /// </summary>
        public async Task<RefreshToken?> GetRefreshTokenWithUserAsync(string token,
                                                                      CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _refreshTokenRepository.GetByTokenWithUserAsync(token, cancellationToken);
        }

        /// <summary>
        /// Removes all expired or revoked refresh tokens for a user.
        /// </summary>
        public async Task<int> RemoveInactiveTokensAsync(User user,
                                                         CancellationToken cancellationToken = default,
                                                         bool commit = true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var inactiveTokens = await _refreshTokenRepository
                                        .GetInactiveTokensByUserIdAsync(user.Id, cancellationToken);

            if (!inactiveTokens.Any())
            {
                return 0;
            }

            foreach (var token in inactiveTokens)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _refreshTokenRepository.Remove(token);
            }

            if (commit)
            {
                await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Removed {Count} inactive refresh tokens for user {UserId}",
                                   inactiveTokens.Count,
                                    user.Id);

            return inactiveTokens.Count;
        }
    }
}
