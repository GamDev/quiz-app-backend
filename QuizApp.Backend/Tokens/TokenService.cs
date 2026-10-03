using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using QuizApp.Backend.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuizApp.Backend.Users;
using Microsoft.AspNetCore.WebUtilities;
using QuizApp.Backend.Tokens.Dtos;

namespace QuizApp.Backend.Tokens
{
    /// <summary>
    /// Provides services for generating JWT access tokens
    /// and cryptographically secure refresh tokens.
    /// </summary>
    public sealed class TokenService : ITokenService
    {
        private static readonly JwtSecurityTokenHandler _tokenHandler = new();
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<TokenService> _logger;
        private readonly SigningCredentials _signingCredentials;
        private const int SecondsPerMinute = 60;
        public TokenService(IOptions<JwtSettings> jwtOptions,
                            ILogger<TokenService> logger)
        {
            _jwtSettings = jwtOptions.Value;
            _logger = logger;

            // Take my secret string and convert it into bytes.
            var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            if (keyBytes.Length < 32)
                throw new InvalidOperationException("JWT signing key must be at least 32 bytes.");

            _signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
        }

        /// <summary>
        /// Generates a signed JWT access token for the specified user.
        /// </summary>
        public string GenerateAccessToken(User user,
                                         IEnumerable<Claim>? additionalClaims = null)
        {

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
                signingCredentials: _signingCredentials
            );


            _logger.LogDebug("Access token generated for user {UserId}", user.Id);
            return _tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generates a cryptographically secure refresh token.
        /// </summary>
        public IssuedRefreshToken GenerateRefreshToken()
        {
            var rawToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
            var now = DateTime.UtcNow;

            var entity = new RefreshToken
            {
                TokenHash = HashToken(rawToken),   // only the hash is persisted
                Created = now,
                Expires = now.AddDays(_jwtSettings.RefreshTokenExpiresDays)
            };

            return new IssuedRefreshToken(entity, rawToken);
        }
        public string HashToken(string rawToken) =>
               Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
               
        /// <summary>
        /// Gets the configured access token lifetime in seconds.
        /// </summary>
        public int AccessTokenExpiryInSeconds => _jwtSettings.AccessTokenExpiresMinutes * SecondsPerMinute;
    }
}