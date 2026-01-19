namespace QuizApp.Backend.Dtos
{
    public record ApiResponse<T>(bool Success, T? Data = default, string? message = null);

}