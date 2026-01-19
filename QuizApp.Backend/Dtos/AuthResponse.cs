namespace QuizApp.Backend.Dtos
{
   public record AuthResponse(string AccessToken, string RefreshToken, int ExpiresInSeconds);

}