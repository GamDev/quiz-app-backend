namespace com.QuizAppBackend.Dtos
{
    public record AuthResult(
        bool IsSuccess,
        string? AccessToken,
        string? RefreshToken,
        string? Error,
        string? UserId = null) 
    {
        public static AuthResult Success(string accessToken, string refreshToken, string? userId = null) =>
            new(true, accessToken, refreshToken, null, userId);

        public static AuthResult Failure(string error) =>
            new(false, null, null, error, null);
    }
}