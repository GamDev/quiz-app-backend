namespace QuizApp.Backend.Quizzes.Dtos
{
    /// <summary>
    /// Represents a request to update an existing quiz.
    /// </summary>
    public record UpdateQuizRequest(string Title,
                                    string Description,
                                    List<UpdateQuestionRequest> Questions);
}