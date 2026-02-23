using Microsoft.AspNetCore.Mvc;
using Project.Core.DTO;
using Project.Core.ServiceContracts;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLogsController : ControllerBase
    {
        private readonly IUserLogService _userLogService;

        public UserLogsController(IUserLogService userLogService)
        {
            _userLogService = userLogService;
        }

        /// <summary>
        /// Creates a new user activity log entry based on the provided data.
        /// </summary>
        /// <param name="dto">The data transfer object containing information about the user activity to log. Cannot be null and must
        /// satisfy all validation requirements.</param>
        /// <returns>An <see cref="OkObjectResult"/> containing a confirmation message if the log is created successfully;
        /// otherwise, a <see cref="BadRequestObjectResult"/> containing validation errors.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateLog([FromBody] CreateUserLogDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _userLogService.LogActivityAsync(dto);

            // بترجع 200 OK إن العملية تمت
            return Ok(new { Message = "Log saved successfully" });
        }
    }
}
