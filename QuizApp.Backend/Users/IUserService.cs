
using QuizApp.Backend.Users.Dtos;

namespace QuizApp.Backend.Users
{
   public interface IUserService
   {
      Task<UserResponse> CreateUserAsync(User user, CancellationToken cancellationToken = default);
      Task<UserResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
      Task<UserResponse?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
      Task<(IReadOnlyList<UserResponse> Items, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 50,
                                                                        CancellationToken cancellationToken = default);
   }

}