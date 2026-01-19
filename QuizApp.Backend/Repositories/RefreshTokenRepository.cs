using QuizApp.Backend.Data;
using QuizApp.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace QuizApp.Backend.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly QuizAppDBContext _dbContext;

        public RefreshTokenRepository(QuizAppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbContext.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        }

        public async Task<RefreshToken?> GetByTokenWithUserAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .SingleOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        }

        public void Add(RefreshToken refreshToken) => _dbContext.RefreshTokens.Add(refreshToken);

        public void Remove(RefreshToken refreshToken) => _dbContext.RefreshTokens.Remove(refreshToken);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
        public async Task<IReadOnlyList<RefreshToken>> GetExpiredTokensByUserIdAsync(int userId,
                                                                                     CancellationToken cancellationToken = default)
        {
            return await _dbContext.RefreshTokens
                         .Where(rt => rt.UserId == userId &&
                         (rt.Revoked != null || rt.Expires <= DateTime.UtcNow))
                         .ToListAsync(cancellationToken);
        }
    }
}