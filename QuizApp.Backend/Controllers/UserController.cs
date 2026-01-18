using com.QuizAppBackend.Dtos;
using com.QuizAppBackend.Interfaces;
using com.QuizAppBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace com.QuizAppBackend.Controllers
{

    [ApiController]
    [Authorize]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1,
                                                      [FromQuery] int pageSize = 50,
                                                      CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _userService.GetUserAllAsync(page, pageSize, cancellationToken);

            // Wrap in PagedResult
            var pagedResult = new PagedResult<UserResponse>(
                items,
                totalCount,
                page,
                pageSize
            );

            // Return API response
            return Ok(new ApiResponse<PagedResult<UserResponse>>(true, pagedResult));
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserById(int id,
                                                     CancellationToken cancellationToken)
        {
            var user = await _userService.GetUserByIdAsync(id, cancellationToken);

            if (user == null)
                return NotFound(new ApiResponse<object>(false, null, "User not found"));

            return Ok(new ApiResponse<object>(true, user));
        }
    }
}
