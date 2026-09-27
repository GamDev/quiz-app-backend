using QuizApp.Backend.Quizzes.Dtos;
using QuizApp.Backend.Quizzes.Models;

namespace QuizApp.Backend.Quizzes
{
    /// <summary>
    /// Provides application-level operations for managing quizzes.
    /// </summary>
    public sealed class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuizService"/> class.
        /// </summary>
        /// <param name="quizRepository">
        /// Repository used to access quiz data.
        /// </param>
        public QuizService(IQuizRepository quizRepository)
        {
            _quizRepository = quizRepository;
        }

        /// <summary>
        /// Retrieves a quiz by its unique identifier.
        /// </summary>
        public async Task<QuizResponse?> GetByIdAsync(int id,
                                                      CancellationToken cancellationToken)
        {
            var quiz = await _quizRepository.GetByIdAsync(id, cancellationToken);

            if (quiz is null)
            {
                return null;
            }

            return MapToResponse(quiz);
        }

        /// <summary>
        /// Retrieves all quizzes.
        /// </summary>
        public async Task<IReadOnlyList<QuizResponse>> GetAllAsync(CancellationToken cancellationToken)
        {
            var quizzes = await _quizRepository.GetAllAsync(cancellationToken);
            return quizzes.Select(MapToResponse).ToList();
        }

        /// <summary>
        /// Creates a new quiz from the supplied request.
        /// </summary>
        public async Task<QuizResponse> CreateAsync(CreateQuizRequest request,
                                                    int createdBy,
                                                    CancellationToken cancellationToken)
        {
            var quiz = new Quiz
            {
                Title = request.Title,
                Description = request.Description,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                IsPublished = false
            };

            foreach (var questionRequest in request.Questions)
            {
                var question = new Question
                {
                    Text = questionRequest.Text
                };

                foreach (var optionRequest in questionRequest.Options)
                {
                    question.Options.Add(
                        new QuestionOption
                        {
                            Text = optionRequest.Text,
                            IsCorrect = optionRequest.IsCorrect
                        });
                }

                quiz.Questions.Add(question);
            }

            var createdQuiz = await _quizRepository.AddAsync(quiz, cancellationToken);

            return MapToResponse(createdQuiz);
        }

        /// <summary>
        /// Updates an existing quiz, including its questions and options.
        /// Existing items are updated, new items are added, and items removed
        /// from the request are removed from the quiz.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the quiz to update.
        /// </param>
        /// <param name="request">
        /// The updated quiz information.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// The updated quiz, or <c>null</c> if the quiz does not exist.
        /// </returns>
        public async Task<QuizResponse?> UpdateAsync(int id,
                                                     UpdateQuizRequest request,
                                                     CancellationToken cancellationToken)
        {
            var quiz = await _quizRepository.GetByIdForUpdateAsync(
                id,
                cancellationToken);

            if (quiz is null)
            {
                return null;
            }

            quiz.Title = request.Title;
            quiz.Description = request.Description;

            foreach (var questionRequest in request.Questions)
            {
                Question question;

                if (questionRequest.Id is null)
                {
                    question = new Question
                    {
                        Text = questionRequest.Text
                    };

                    quiz.Questions.Add(question);
                }
                else
                {
                    question = quiz.Questions.Single(question =>
                                              question.Id == questionRequest.Id.Value);

                    question.Text = questionRequest.Text;
                }

                foreach (var optionRequest in questionRequest.Options)
                {
                    if (optionRequest.Id is null)
                    {
                        question.Options.Add(
                            new QuestionOption
                            {
                                Text = optionRequest.Text,
                                IsCorrect = optionRequest.IsCorrect
                            });

                        continue;
                    }

                    var option = question.Options.Single(option =>
                                    option.Id == optionRequest.Id.Value);

                    option.Text = optionRequest.Text;
                    option.IsCorrect = optionRequest.IsCorrect;
                }

                var requestedOptionIds = questionRequest.Options
                    .Where(option => option.Id.HasValue)
                    .Select(option => option.Id.Value)
                    .ToHashSet();

                var optionsToRemove = question.Options
                    .Where(option =>
                        option.Id != 0 &&
                        !requestedOptionIds.Contains(option.Id))
                    .ToList();

                foreach (var option in optionsToRemove)
                {
                    question.Options.Remove(option);
                }
            }

            var requestedQuestionIds = request.Questions
                .Where(question => question.Id.HasValue)
                .Select(question => question.Id.Value)
                .ToHashSet();

            var questionsToRemove = quiz.Questions
                .Where(question =>
                    question.Id != 0 &&
                    !requestedQuestionIds.Contains(question.Id))
                .ToList();

            foreach (var question in questionsToRemove)
            {
                quiz.Questions.Remove(question);
            }

            await _quizRepository.SaveChangesAsync(cancellationToken);

            return MapToResponse(quiz);
        }
        /// <summary>
        /// Deletes a quiz by its unique identifier.
        /// </summary>
        public async Task DeleteAsync(int id,
                                      CancellationToken cancellationToken)
        {
            var quiz = await _quizRepository.GetByIdAsync(id, cancellationToken);

            if (quiz is null)
            {
                return;
            }

            await _quizRepository.DeleteAsync(quiz, cancellationToken);
        }

        /// <summary>
        /// Maps a quiz entity to the response model exposed by the API.
        /// </summary>
        /// <param name="quiz">
        /// The quiz entity to map.
        /// </param>
        /// <returns>
        /// A response containing the quiz and its questions and options.
        /// </returns>
        private static QuizResponse MapToResponse(Quiz quiz)
        {
            var questions = quiz.Questions
                .Select(question =>
                    new QuestionResponse(
                        question.Id,
                        question.Text,
                        question.Options
                            .Select(option =>
                                new QuestionOptionResponse(
                                    option.Id,
                                    option.Text))
                            .ToList()))
                .ToList();

            return new QuizResponse(
                quiz.Id,
                quiz.Title,
                quiz.Description,
                quiz.IsPublished,
                quiz.CreatedAt,
                questions);
        }
    }
}