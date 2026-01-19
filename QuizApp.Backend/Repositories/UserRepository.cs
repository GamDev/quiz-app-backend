

using QuizApp.Backend.Data;
using QuizApp.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace QuizApp.Backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly QuizAppDBContext _dbContext;

        public UserRepository(QuizAppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetAllAsync(int page,
                                                                                   int pageSize,
                                                                                   CancellationToken cancellationToken)
        {
            var query = _dbContext.Users.AsNoTracking();

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
        }
    }
}
