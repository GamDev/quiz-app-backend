namespace QuizApp.Backend.Users.Dtos
{
    public record UserResponse(int Id, string FullName, string Email,string Role, string CreatedAt);
}
