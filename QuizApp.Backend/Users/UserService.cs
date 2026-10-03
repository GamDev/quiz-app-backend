using QuizApp.Backend.Common;
using QuizApp.Backend.Users.Dtos;

namespace QuizApp.Backend.Users
{
    /// <summary>
    /// Provides application-level operations for managing users.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userRepository">Repository used to access user data.</param>
        /// <param name="logger">Logger used to record user-related operations.</param>
        public UserService(IUserRepository userRepository,
                           ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new user and returns the corresponding response DTO.
        /// </summary>
        /// <param name="user">The user entity to create.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The created user's response DTO.</returns>
        public async Task<UserResponse> CreateUserAsync(User user,
                                                        CancellationToken cancellationToken = default)
        {
            var newUser = await _userRepository.AddAsync(user, cancellationToken);

            _logger.LogInformation("User created successfully: {UserId}", newUser.Id);

            return MapToResponse(newUser);
        }

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>
        /// The user's response DTO if found; otherwise, <see langword="null"/>.
        /// </returns>
        public async Task<UserResponse?> GetByEmailAsync(string email,
                                                        CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            return user is null ? null : MapToResponse(user);
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>
        /// The user's response DTO if found; otherwise, <see langword="null"/>.
        /// </returns>
        public async Task<UserResponse?> GetUserByIdAsync(int userId,
                                                          CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            return user is null ? null : MapToResponse(user);
        }

        /// <summary>
        /// Retrieves a paginated list of users along with the total number of users.
        /// </summary>
        /// <param name="page">The page number, starting from 1.</param>
        /// <param name="pageSize">The maximum number of users to return per page.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>
        /// A tuple containing the paginated user response DTOs and the total user count.
        /// </returns>
        public async Task<(IReadOnlyList<UserResponse> Items, int TotalCount)> GetAllAsync(
                                                                                int page = 1,
                                                                                int pageSize = 50,
                                                                                CancellationToken cancellationToken = default)
        {
            var (users, totalCount) = await _userRepository.GetAllAsync(page, pageSize, cancellationToken);

            var userResponses = users.Select(MapToResponse).ToList();

            return (userResponses, totalCount);
        }

        /// <summary>
        /// Maps a <see cref="User"/> entity to a <see cref="UserResponse"/> DTO.
        /// </summary>
        /// <param name="user">The user entity to map.</param>
        /// <returns>A response DTO containing the user's public information.</returns>
        private static UserResponse MapToResponse(User user)
        {
            return new UserResponse(
                user.Id,
                user.FullName,
                user.Email,
                user.Role.ToString(),
                user.CreatedAt);
        }
        /// <summary>
        /// Retrieves the underlying <see cref="User"/> domain entity by email.
        /// Used exclusively for internal service operations (e.g., Auth, Password Verification, JWT).
        /// </summary>
        public async Task<User?> GetUserEntityByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByEmailAsync(EmailNormalizer.Normalize(email), cancellationToken);
        }
        public Task<bool> EmailExistsAsync(string email,
                                           CancellationToken cancellationToken = default) =>
                         _userRepository.EmailExistsAsync(EmailNormalizer.Normalize(email), cancellationToken);
    }
}