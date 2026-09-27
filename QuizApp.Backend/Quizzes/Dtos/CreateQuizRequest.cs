namespace QuizApp.Backend.Quizzes.Dtos
{
    public record CreateQuizRequest(string Title,
                                    string Description,
                                    List<CreateQuestionRequest> Questions);
}