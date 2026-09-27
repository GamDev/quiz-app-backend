namespace QuizApp.Backend.Quizzes.Dtos
{
    public record CreateQuestionOptionRequest(string Text,
                                              bool IsCorrect);
}