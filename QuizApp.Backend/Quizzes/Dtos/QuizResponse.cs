namespace QuizApp.Backend.Quizzes.Dtos
{
        /// <summary>
    /// Represents quiz information exposed by the API.
    /// </summary>
    public record QuizResponse(int Id,
                                string Title,
                                string Description,
                                bool IsPublished,
                                DateTime CreatedAt,
                                List<QuestionResponse> Questions);
}