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
        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbContext.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        }
        /// <summary>
        /// Retrieves a refresh token by its token value,
        /// including the associated user.
        /// </summary>
        public async Task<RefreshToken?> GetByTokenWithUserAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .SingleOrDefaultAsync(rt => rt.Token == token, cancellationToken);
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
            return await _dbContext.RefreshTokens
                         .Where(rt => rt.UserId == userId &&
                         (rt.Revoked != null || rt.Expires <= DateTime.UtcNow))
                         .ToListAsync(cancellationToken);
        }
    }
}