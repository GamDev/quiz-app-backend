namespace QuizApp.Backend.Quizzes.Dtos
{
    /// <summary>
    /// Represents question information exposed by the API.
    /// </summary>
    public record QuestionResponse(int Id,
                                    string Text,
                                    List<QuestionOptionResponse> Options);


}