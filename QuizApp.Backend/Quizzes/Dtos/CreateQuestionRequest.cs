namespace QuizApp.Backend.Quizzes.Dtos
{
     public record CreateQuestionRequest(string Text,
                                         List<CreateQuestionOptionRequest> Options);
}