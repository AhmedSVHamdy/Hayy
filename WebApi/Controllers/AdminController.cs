using Microsoft.AspNetCore.Mvc;
using Project.Core.ServiceContracts;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // 🔐 يفضل تحط دي عشان مش أي حد يوافق
    public class AdminController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public AdminController(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        // Endpoint للموافقة على البيزنس
        [HttpPatch("businesses/{id}/approve")]
        public async Task<IActionResult> ApproveBusiness(Guid id)
        {
            var resultDto = await _businessService.ApproveBusinessProfile(id);

            if (resultDto == null)
                return NotFound(new { Message = "البيزنس غير موجود" });

            // بنرجع 200 OK ومعاه الـ JSON الجديد
            return Ok(resultDto);
        }
    }
}
