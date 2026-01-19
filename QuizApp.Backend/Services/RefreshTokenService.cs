using com.QuizAppBackend.Interfaces;
using com.QuizAppBackend.Models;
using com.QuizAppBackend.Repositories;

namespace com.QuizAppBackend.Services
{
    public class RefreshTokenService : IRefreshTokenService
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
        public async Task<RefreshToken> RotateAsync(User user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var newToken = _tokenService.GenerateRefreshToken();
            newToken.UserId = user.Id;
            _refreshTokenRepository.Add(newToken);
            
            await RemoveExpiredTokensAsync(user, cancellationToken, commit: false);

            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Rotated refresh token for user {UserId}, new token expires at {Expiry}",
                user.Id, newToken.Expires);

            return newToken;
        }

        /// <summary>
        /// Revokes a refresh token.
        /// </summary>
        public async Task<bool> RevokeAsync(string token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var refreshToken = await _refreshTokenRepository.GetByTokenWithUserAsync(token, cancellationToken);
            if (refreshToken == null || !refreshToken.IsActive)
            {
                _logger.LogWarning(
                    "Attempted to revoke invalid or inactive token: {Token}", token);
                return false;
            }

            refreshToken.Revoked = DateTime.UtcNow;

            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Revoked refresh token for user {UserId}, token {Token}",
                refreshToken.User?.Id, refreshToken.Token);

            return true;
        }

        /// <summary>
        /// Removes all expired or inactive refresh tokens for a user.
        /// </summary>
        public async Task<int> RemoveExpiredTokensAsync(User user,
                                                        CancellationToken cancellationToken = default,
                                                        bool commit = true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var expiredTokens =
              await _refreshTokenRepository.GetExpiredTokensByUserIdAsync(user.Id, cancellationToken);
            if (!expiredTokens.Any()) return 0;

            foreach (var token in expiredTokens)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _refreshTokenRepository.Remove(token);
            }

            if (commit)
                await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Removed {Count} expired refresh tokens for user {UserId}",
                expiredTokens.Count, user.Id);

            return expiredTokens.Count;
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
    }
}
