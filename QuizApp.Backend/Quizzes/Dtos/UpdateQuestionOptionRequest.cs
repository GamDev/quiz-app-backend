namespace QuizApp.Backend.Quizzes.Dtos
{
      /// <summary>
      /// Represents an option update within a quiz question.
      /// </summary>
      public record UpdateQuestionOptionRequest(int? Id,
                                                string Text,
                                                bool IsCorrect);

}