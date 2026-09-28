namespace QuizApp.Backend.Auth.Dtos
{
    public record AuthResult(bool IsSuccess,
                            string? AccessToken,
                            string? RefreshToken,
                            DateTime? RefreshTokenExpiresAt,
                            string? Error) 
    {
        public static AuthResult Success(string accessToken, string refreshToken,   DateTime refreshTokenExpiresAt) =>
                                         new(true, accessToken, refreshToken,refreshTokenExpiresAt, null);

        public static AuthResult Failure(string error) =>
                                 new(false, null, null,null, error);
    }
}