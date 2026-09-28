
namespace QuizApp.Backend.Common
{

    public interface ITransactionManager
    {
        Task BeginAsync(CancellationToken cancellationToken = default);

        Task CommitAsync(CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}