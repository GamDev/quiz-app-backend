using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizApp.Backend.Common;
using QuizApp.Backend.Quizzes.Dtos;

namespace QuizApp.Backend.Quizzes
{
    /// <summary>
    /// Provides HTTP endpoints for managing quizzes.
    /// </summary>

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }


        /// <summary>
        /// Retrieves all quizzes.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token used to cancel the request.
        /// </param>
        /// <returns>
        /// A response containing all quizzes.
        /// </returns>

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var quizzes = await _quizService.GetAllAsync(cancellationToken);
            return Ok(new ApiResponse<IReadOnlyList<QuizResponse>>(true, quizzes));
        }

        /// <summary>
        /// Retrieves a quiz by its unique identifier.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the quiz.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the request.
        /// </param>
        /// <returns>
        /// A response containing the requested quiz,
        /// or a not-found response if the quiz does not exist.
        /// </returns>

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var quiz = await _quizService.GetByIdAsync(id, cancellationToken);

            if (quiz == null)
            {
                return NotFound(new ApiResponse<QuizResponse>(false, message: "Quiz not found"));
            }

            return Ok(new ApiResponse<QuizResponse>(true, quiz));
        }

        /// <summary>
        /// Creates a new quiz.
        /// </summary>
        /// <param name="request">
        /// The quiz information supplied by the client.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the request.
        /// </param>
        /// <returns>
        /// The newly created quiz.
        /// </returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateQuizRequest request,
                                               CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int userId))
            {
                return Unauthorized(new ApiResponse<QuizResponse>(false, message: "Invalid or missing user Identity"));
            }
            var quiz = await _quizService.CreateAsync(request, userId, cancellationToken);
            // Your Quiz was successfully created, and here is the endpoint
            // where you can retrieve it.
            return CreatedAtAction(nameof(GetById),
                                   new { id = quiz.Id },
                                    new ApiResponse<QuizResponse>(true, quiz));

        }



        /// <summary>
        /// Updates an existing quiz.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the quiz to update.
        /// </param>
        /// <param name="request">
        /// The updated quiz data.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns the updated quiz when found; otherwise, returns 404 Not Found.
        /// </returns>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id,
                                                [FromBody] UpdateQuizRequest request,
                                                 CancellationToken cancellationToken)
        {
            var quiz = await _quizService.UpdateAsync(id, request, cancellationToken);

            if (quiz is null)
            {
                return NotFound(new ApiResponse<QuizResponse>(false, message: "Quiz not found."));
            }

            return Ok(new ApiResponse<QuizResponse>(true, quiz));
        }

        /// <summary>
        /// Deletes an existing quiz.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the quiz to delete.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// Returns 204 No Content after the quiz is deleted.
        /// </returns>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id,
                                                CancellationToken cancellationToken)
        {
            await _quizService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Safely extracts the current authenticated user's ID from claims.
        /// </summary>
        private bool TryGetUserId(out int userId)
        {
            var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claimValue, out userId);
        }
    }
}