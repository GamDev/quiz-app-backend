namespace QuizApp.Backend.Auth.Dtos
{
   public record AuthResponse(string AccessToken, string RefreshToken, int ExpiresInSeconds);

}