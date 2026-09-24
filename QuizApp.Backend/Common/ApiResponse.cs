namespace QuizApp.Backend.Common
{
    public record ApiResponse<T>(bool Success, T? Data = default, string? message = null);

}