namespace QuizApp.Backend.Auth.Dtos
{
    public record AuthResult(bool IsSuccess,
                            string? AccessToken,
                            string? RefreshToken,
                            string? Error) 
    {
        public static AuthResult Success(string accessToken, string refreshToken, string? userId = null) =>
                                         new(true, accessToken, refreshToken, null);

        public static AuthResult Failure(string error) =>
                                 new(false, null, null, error);
    }
}