namespace com.QuizAppBackend.Dtos
{
    public record UserResponse(int Id, string FullName, string Email, DateTime CreatedAt);
}
