using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using QuizApp.Backend.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuizApp.Backend.Users;

namespace QuizApp.Backend.Tokens
{
    /// <summary>
    /// Provides services for generating JWT access tokens
    /// and cryptographically secure refresh tokens.
    /// </summary>
    public sealed class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<TokenService> _logger;
        private const int SecondsPerMinute = 60;
        public TokenService(IOptions<JwtSettings> jwtOptions,
                            ILogger<TokenService> logger)
        {
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
        }
        /// <summary>
        /// Generates a signed JWT access token for the specified user.
        /// </summary>
        public string GenerateAccessToken(User user,
                                         IEnumerable<Claim>? additionalClaims = null)
        {

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
                 {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("token_type", "access"),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                };

            if (additionalClaims != null)
                claims.AddRange(additionalClaims);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiresMinutes),
                signingCredentials: credentials
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Access token generated for user {UserId}, expires at {Expiry}",
                                   user.Id,
                                   token.ValidTo
            );

            return jwt;
        }

        /// <summary>
        /// Generates a cryptographically secure refresh token.
        /// </summary>
        public RefreshToken GenerateRefreshToken()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshToken = new RefreshToken
            {
                Token = token,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresDays)
            };

            _logger.LogInformation("Refresh token generated, expires at {Expiry}",
                               refreshToken.Expires);

            return refreshToken;
        }

        /// <summary>
        /// Gets the configured access token lifetime in seconds.
        /// </summary>
        public int AccessTokenExpiryInSeconds => _jwtSettings.AccessTokenExpiresMinutes * SecondsPerMinute;
    }
}