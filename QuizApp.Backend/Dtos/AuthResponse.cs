namespace com.QuizAppBackend.Dtos
{
   public record AuthResponse(string AccessToken, string RefreshToken, int ExpiresInSeconds);

}