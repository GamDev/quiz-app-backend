namespace QuizApp.Backend.Auth.Dtos
{
   public record AuthResponse(string AccessToken, int ExpiresInSeconds);

}