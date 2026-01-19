
using QuizApp.Backend.Dtos;
using QuizApp.Backend.Interfaces;
using QuizApp.Backend.Models;
using QuizApp.Backend.Repositories;


namespace QuizApp.Backend.Services
{

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User?> CreateUser(User? user,
                                            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                _logger.LogWarning("Attempted to create invalid user");
                return null;
            }

            try
            {
                var createdUser = await _userRepository.AddAsync(user, cancellationToken);
                _logger.LogInformation("User created successfully: {UserId}", createdUser.Id);
                return createdUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with email {Email}", user.Email);
                throw;
            }
        }

        public async Task<User?> GetByEmailAsync(string email,
                                                 CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _userRepository.GetByEmailAsync(email, cancellationToken);
        }

        public async Task<User?> GetUserByIdAsync(int userId,
                                                  CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _userRepository.GetByIdAsync(userId, cancellationToken);
        }

        public async Task<(IReadOnlyList<UserResponse> Items, int TotalCount)> GetUserAllAsync(int page = 1,
                                                                      int pageSize = 50,
                                                                      CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (users, totalCount) = await _userRepository.GetAllAsync(page, pageSize, cancellationToken);

            // Map EF entities to DTOs
            var userResponses = users
                .Select(u => new UserResponse(u.Id, u.FullName, u.Email, u.CreatedAt))
                .ToList();

            return (userResponses, totalCount);
        }
    }
}