
namespace QuizApp.Backend.Tokens
{

    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetByTokenWithUserAsync(string token, CancellationToken cancellationToken = default);
        public void Add(RefreshToken refreshToken);
        public void Remove(RefreshToken refreshToken);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<RefreshToken>> GetInactiveTokensByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> TryRevokeAsync(int tokenId, string replacedByToken, CancellationToken cancellationToken = default);
        Task<int> RevokeAllActiveForUserAsync(int userId, CancellationToken cancellationToken = default);
    }

}