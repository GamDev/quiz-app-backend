using QuizApp.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace QuizApp.Backend.Tokens
{
    /// <summary>
    /// Provides database operations for refresh tokens.
    /// </summary>
    public sealed class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly QuizAppDBContext _dbContext;
        public RefreshTokenRepository(QuizAppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a refresh token by its token value.
        /// </summary>
        public async Task<RefreshToken?> GetByTokenHashAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbContext.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.TokenHash == token, cancellationToken);
        }
        /// <summary>
        /// Retrieves a refresh token by its token value,
        /// including the associated user.
        /// </summary>
        public async Task<RefreshToken?> GetByTokenHashWithUserAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .SingleOrDefaultAsync(rt => rt.TokenHash == token, cancellationToken);
        }

        /// <summary>
        /// Adds a refresh token to the current database context.
        /// </summary>
        public void Add(RefreshToken refreshToken)
        {
            _dbContext.RefreshTokens.Add(refreshToken);
        }

        /// <summary>
        /// Removes a refresh token from the current database context.
        /// </summary>
        public void Remove(RefreshToken refreshToken)
        {
            _dbContext.RefreshTokens.Remove(refreshToken);
        }

        /// <summary>
        /// Persists pending refresh token changes to the database.
        /// </summary>
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves inactive refresh tokens for a user.
        /// Inactive tokens are either revoked or expired.
        /// </summary>
        public async Task<IReadOnlyList<RefreshToken>> GetInactiveTokensByUserIdAsync(int userId,
                                                                                     CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var cutoff = now.AddDays(-7); // keep revoked tokens for 7 days

            return await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId &&
                             (rt.Expires <= now ||
                              (rt.Revoked != null && rt.Revoked < cutoff)))
                .ToListAsync(cancellationToken);
        }
        public async Task<bool> TryRevokeAsync(int tokenId, string replacedByToken, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var rows = await _dbContext.RefreshTokens
                .Where(t => t.Id == tokenId && t.Revoked == null && t.Expires > now)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.Revoked, now)
                    .SetProperty(t => t.ReplacedByTokenHash, replacedByToken),
                    cancellationToken);

            return rows == 1;   // 0 means another request already used this token
        }
        public async Task<int> RevokeAllActiveForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await _dbContext.RefreshTokens
                .Where(t => t.UserId == userId && t.Revoked == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.Revoked, now), cancellationToken);
        }
    }
}