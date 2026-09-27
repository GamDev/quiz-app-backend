namespace QuizApp.Backend.Quizzes.Models
{
    /// <summary>
    /// Represents a question belonging to a quiz.
    /// </summary>
    public class Question
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Text { get; set; } = string.Empty;
        public Quiz Quiz { get; set; } = null!;
        public ICollection<QuestionOption> Options { get; set; }  = new List<QuestionOption>();
    }
}