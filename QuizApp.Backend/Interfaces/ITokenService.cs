using System.Security.Claims;
using com.QuizAppBackend.Models;

namespace com.QuizAppBackend.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, IEnumerable<Claim>? additionalClaims = null);
        RefreshToken GenerateRefreshToken();
        int AccessTokenExpiryInSeconds { get; }
    }
}