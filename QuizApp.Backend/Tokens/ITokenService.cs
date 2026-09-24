using System.Security.Claims;
using QuizApp.Backend.Users;


namespace QuizApp.Backend.Tokens
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, IEnumerable<Claim>? additionalClaims = null);
        RefreshToken GenerateRefreshToken();
        int AccessTokenExpiryInSeconds { get; }
    }
}