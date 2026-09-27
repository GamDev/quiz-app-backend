namespace QuizApp.Backend.Users.Dtos
{
    /// <summary>
    /// Represents the user information exposed by the API.
    /// </summary>
    public record UserResponse(int Id,
                             string FullName,
                             string Email,
                             string Role,
                             DateTime CreatedAt);
}
