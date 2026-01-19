using System.Security.Claims;
using QuizApp.Backend.Models;

namespace QuizApp.Backend.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, IEnumerable<Claim>? additionalClaims = null);
        RefreshToken GenerateRefreshToken();
        int AccessTokenExpiryInSeconds { get; }
    }
}