using QuizApp.Backend.Models;

namespace QuizApp.Backend.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> RotateAsync(User user,CancellationToken cancellationToken = default);
        Task<bool> RevokeAsync(string token,CancellationToken cancellationToken = default);
        Task<int> RemoveExpiredTokensAsync(User user, CancellationToken cancellationToken = default, bool commit = true);
        Task<RefreshToken?> GetRefreshTokenWithUserAsync(string token,CancellationToken cancellationToken = default);
    }
}