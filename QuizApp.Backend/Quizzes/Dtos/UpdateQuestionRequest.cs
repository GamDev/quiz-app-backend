namespace QuizApp.Backend.Quizzes.Dtos
{
    /// <summary>
    /// Represents a question update within a quiz.
    /// </summary>
 public record UpdateQuestionRequest(int? Id,
                                     string Text,
                                     List<UpdateQuestionOptionRequest> Options);
}