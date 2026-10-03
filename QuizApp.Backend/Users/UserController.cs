using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizApp.Backend.Common;
using QuizApp.Backend.Users.Dtos;

namespace QuizApp.Backend.Users
{
    /// <summary>
    /// Provides HTTP endpoints for user management.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public sealed class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retrieves a paginated list of non-admin users. Requires Admin role.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _userService.GetAllAsync(page, pageSize, cancellationToken);

            var pagedResult = new PagedResult<UserResponse>(items, totalCount, page, pageSize);

            return Ok(new ApiResponse<PagedResult<UserResponse>>(true, pagedResult));
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserById(int id,
                                                    CancellationToken cancellationToken)
        {
            var user = await _userService.GetUserByIdAsync(id, cancellationToken);

            if (user is null)
            {
                return NotFound(new ApiResponse<UserResponse>(false, message: "User not found"));
            }

            return Ok(new ApiResponse<UserResponse>(true, user));
        }
    }
}