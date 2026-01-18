namespace com.QuizAppBackend.Dtos
{
    public record ApiResponse<T>(bool Success, T? Data = default, string? message = null);

}