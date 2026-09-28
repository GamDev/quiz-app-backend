
using QuizApp.Backend.Users;

namespace QuizApp.Backend.Tokens
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> CreateAsync(User user, CancellationToken cancellationToken = default);

        Task<RefreshToken?> RotateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

        Task<bool> RevokeAsync(string token, CancellationToken cancellationToken = default);

        Task<int> RemoveInactiveTokensAsync(User user, CancellationToken cancellationToken = default, bool commit = true);

        Task<RefreshToken?> GetRefreshTokenWithUserAsync(string token, CancellationToken cancellationToken = default);
        Task RevokeAllForUserAsync(int userId, CancellationToken cancellationToken = default);

    }
}