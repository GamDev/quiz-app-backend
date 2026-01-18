
using com.QuizAppBackend.Dtos;
using com.QuizAppBackend.Models;

namespace com.QuizAppBackend.Interfaces
{
   public interface IUserService
   {
      Task<User?> CreateUser(User? user, CancellationToken cancellationToken = default);
      Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
      Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
       Task<(IReadOnlyList<UserResponse> Items, int TotalCount)> GetUserAllAsync(int page = 1, int pageSize = 50,
                                                                        CancellationToken cancellationToken = default);
   }

}