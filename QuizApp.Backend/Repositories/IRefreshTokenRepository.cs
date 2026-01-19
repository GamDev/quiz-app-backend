
using QuizApp.Backend.Models;
using System.Threading;
using System.Threading.Tasks;

namespace QuizApp.Backend.Repositories
{

    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetByTokenWithUserAsync(string token, CancellationToken cancellationToken = default);
        public void Add(RefreshToken refreshToken);
        public void Remove(RefreshToken refreshToken);
         Task SaveChangesAsync(CancellationToken cancellationToken = default); 
         Task<IReadOnlyList<RefreshToken>> GetExpiredTokensByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    }

}