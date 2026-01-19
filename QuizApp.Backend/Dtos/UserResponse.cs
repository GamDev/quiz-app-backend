namespace QuizApp.Backend.Dtos
{
    public record UserResponse(int Id, string FullName, string Email, DateTime CreatedAt);
}
