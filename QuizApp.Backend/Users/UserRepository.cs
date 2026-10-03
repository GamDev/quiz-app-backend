
using QuizApp.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace QuizApp.Backend.Users
{
    /// <summary>
    /// Provides database operations for users.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly QuizAppDBContext _dbContext;
        public UserRepository(QuizAppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Adds a user to the database.
        /// </summary>
        public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }


        /// <summary>
        /// Retrieves users using pagination.
        /// </summary>
        public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetAllAsync(int page,
                                                                                   int pageSize = 50,
                                                                                   CancellationToken cancellationToken = default)
        {
            // guard againt invalid pagination input
            page = page < 1 ? 1: page;
            pageSize = pageSize<1 ? 1 : pageSize;

            var query = _dbContext.Users.AsNoTracking().Where(x => x.Role != UserRole.Admin);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            string normalizedEmail = email.Trim().ToLowerInvariant();
            return await _dbContext.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }
    }
}
