using com.QuizAppBackend.Models;

namespace com.QuizAppBackend.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<User> Items, int TotalCount)>GetAllAsync(int page = 1,
                                                                     int pageSize = 50, 
                                                                     CancellationToken cancellationToken = default);
    }
}
