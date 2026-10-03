using System.Security.Claims;
using QuizApp.Backend.Tokens.Dtos;
using QuizApp.Backend.Users;


namespace QuizApp.Backend.Tokens
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, IEnumerable<Claim>? additionalClaims = null);
         IssuedRefreshToken GenerateRefreshToken();
        string HashToken(string rawToken);
        int AccessTokenExpiryInSeconds { get; }
    }
}