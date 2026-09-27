using QuizApp.Backend.Quizzes.Dtos;
using QuizApp.Backend.Quizzes.Models;

namespace QuizApp.Backend.Quizzes
{
    public interface IQuizService
    {
         Task<QuizResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);

       Task<IReadOnlyList<QuizResponse>> GetAllAsync(CancellationToken cancellationToken);

        Task<QuizResponse> CreateAsync(CreateQuizRequest request, int createdBy, CancellationToken cancellationToken);

        Task<QuizResponse?> UpdateAsync(int id, UpdateQuizRequest request, CancellationToken cancellationToken);

        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}
