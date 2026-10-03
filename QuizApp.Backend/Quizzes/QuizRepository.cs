using Microsoft.EntityFrameworkCore;
using QuizApp.Backend.Data;
using QuizApp.Backend.Quizzes.Models;

namespace QuizApp.Backend.Quizzes
{
    /// <summary>
    /// Provides database operations for quizzes.
    /// </summary>
    public class QuizRepository : IQuizRepository
    {
        private readonly QuizAppDBContext _dbContext;
        public QuizRepository(QuizAppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a quiz by its unique identifier.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the quiz.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the database operation.
        /// </param>
        /// <returns>
        /// The matching quiz, or <c>null</c> if no quiz exists.
        /// </returns>
        public async Task<Quiz?> GetByIdAsync(int id,
                                              CancellationToken cancellationToken)
        {
            return await _dbContext.Quizzes
              .AsNoTracking()
              .Include(quiz => quiz.Questions)
              .ThenInclude(question => question.Options)
              .FirstOrDefaultAsync(
                  quiz => quiz.Id == id,
                  cancellationToken);
        }

        /// <summary>
        /// Retrieves all quizzes.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token used to cancel the database operation.
        /// </param>
        /// <returns>
        /// A read-only collection containing all quizzes.
        /// </returns>
        public async Task<IReadOnlyList<Quiz>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Quizzes.AsNoTracking()
                                           .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Adds a new quiz to the database.
        /// </summary>
        /// <param name="quiz">
        /// The quiz entity to add.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the database operation.
        /// </param>
        /// <returns>
        /// The created quiz.
        /// </returns>
        public async Task<Quiz> AddAsync(Quiz quiz,
                                        CancellationToken cancellationToken)
        {
            await _dbContext.Quizzes.AddAsync(quiz, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return quiz;
        }


        /// <summary>
        /// Updates an existing quiz in the database.
        /// </summary>
        /// <param name="quiz">
        /// The quiz entity to update.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the database operation.
        /// </param>
        public async Task UpdateAsync(Quiz quiz,
                                      CancellationToken cancellationToken)
        {
            _dbContext.Quizzes.Update(quiz);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Deletes a quiz by its unique identifier.
        /// </summary>
        /// <param name="quizId">The unique identifier of the quiz to delete.</param>
        /// <param name="cancellationToken">Token used to cancel the database operation.</param>
        /// <returns><c>true</c> if a quiz was deleted; otherwise <c>false</c>.</returns>
        public async Task<bool> DeleteAsync(int quizId, CancellationToken cancellationToken)
        {
            var rows = await _dbContext.Quizzes
                .Where(quiz => quiz.Id == quizId)
                .ExecuteDeleteAsync(cancellationToken);

            return rows > 0;
        }
        /// <summary>
        /// Retrieves a quiz with its questions and options for modification.
        /// </summary>
        public async Task<Quiz?> GetByIdForUpdateAsync(int id,
                                                       CancellationToken cancellationToken)
        {
            return await _dbContext.Quizzes
                .Include(quiz => quiz.Questions)
                .ThenInclude(question => question.Options)
                .FirstOrDefaultAsync(
                    quiz => quiz.Id == id,
                    cancellationToken);
        }

        /// <summary>
        /// Persists changes made to tracked entities.
        /// </summary>
        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

    }
}