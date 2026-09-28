using Microsoft.EntityFrameworkCore.Storage;
using QuizApp.Backend.Common;

namespace QuizApp.Backend.Data
{
    /// <summary>
    /// Provides Entity Framework Core based transaction management.
    /// </summary>
    public sealed class TransactionManager : ITransactionManager
    {
        private readonly QuizAppDBContext _dbContext;

        private IDbContextTransaction? _transaction;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="TransactionManager"/> class.
        /// </summary>
        /// <param name="dbContext">
        /// The database context used to manage transactions.
        /// </param>
        public TransactionManager(QuizAppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Begins a new database transaction.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token used to cancel the asynchronous operation.
        /// </param>
        public async Task BeginAsync( CancellationToken cancellationToken = default)
        {
            _transaction =  await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        /// <summary>
        /// Commits the current database transaction.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token used to cancel the asynchronous operation.
        /// </param>
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                return;
            }

            await _transaction.CommitAsync( cancellationToken);

            await _transaction.DisposeAsync();

            _transaction = null;
        }

        /// <summary>
        /// Rolls back the current database transaction.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token used to cancel the asynchronous operation.
        /// </param>
        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                return;
            }

            await _transaction.RollbackAsync( cancellationToken);

            await _transaction.DisposeAsync();

            _transaction = null;
        }
    }
}