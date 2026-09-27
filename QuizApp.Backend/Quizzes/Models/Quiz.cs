using QuizApp.Backend.Users;

namespace QuizApp.Backend.Quizzes.Models
{
    /// <summary>
    /// Represents a quiz stored in the database.
    /// </summary>
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public User CreatedByUser { get; set; } = null!;
        public ICollection<Question> Questions { get; set; }
            = new List<Question>();
    }
}
