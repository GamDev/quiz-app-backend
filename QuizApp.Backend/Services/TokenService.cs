using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using QuizApp.Backend.Interfaces;
using QuizApp.Backend.Models;
using QuizApp.Backend.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace QuizApp.Backend.Services
{
    public class TokenService : ITokenService
    {

        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IOptions<JwtSettings> jwtOptions, ILogger<TokenService> logger)
        {
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
        }

        public string GenerateAccessToken(User user, IEnumerable<Claim>? additionalClaims = null)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                 {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("typ", "access") 
                };

                if (additionalClaims != null)
                    claims.AddRange(additionalClaims);

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiresMinutes),
                    signingCredentials: creds
                );

                var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                _logger.LogInformation(
                    "Access token generated for user {UserId}, expires at {Expiry}",
                    user.Id,
                    token.ValidTo
                );

                return jwt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate access token for user {UserId}", user.Id);
                throw new InvalidOperationException("Could not generate JWT token", ex);
            }
        }

        public RefreshToken GenerateRefreshToken()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshToken = new RefreshToken
            {
                Token = token,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresDays)
            };

            _logger.LogInformation(
                "Refresh token generated, expires at {Expiry}",
                refreshToken.Expires
            );

            return refreshToken;
        }

        public int AccessTokenExpiryInSeconds => _jwtSettings.AccessTokenExpiresMinutes * 60;
    }
}