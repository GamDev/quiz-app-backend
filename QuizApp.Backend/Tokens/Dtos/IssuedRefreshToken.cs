namespace QuizApp.Backend.Tokens.Dtos
{
   public sealed record IssuedRefreshToken(RefreshToken Entity, string RawToken);

}