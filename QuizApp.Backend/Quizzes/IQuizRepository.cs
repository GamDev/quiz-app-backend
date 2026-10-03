using QuizApp.Backend.Quizzes.Models;

namespace QuizApp.Backend.Quizzes
{
    public interface IQuizRepository
    {
        Task<Quiz?> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task<Quiz?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken);

        Task<IReadOnlyList<Quiz>> GetAllAsync(CancellationToken cancellationToken);

        Task<Quiz> AddAsync(Quiz quiz, CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);

        Task<bool> DeleteAsync(int quizId, CancellationToken cancellationToken);
    }
}