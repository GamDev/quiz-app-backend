
namespace QuizApp.Backend.Tokens
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetByTokenHashWithUserAsync(string tokenHash, CancellationToken cancellationToken = default);
        void Add(RefreshToken refreshToken);
        void Remove(RefreshToken refreshToken);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<RefreshToken>> GetInactiveTokensByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> TryRevokeAsync(int tokenId, string replacedByTokenHash, CancellationToken cancellationToken = default);
        Task<int> RevokeAllActiveForUserAsync(int userId, CancellationToken cancellationToken = default);
    }
}