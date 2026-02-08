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
