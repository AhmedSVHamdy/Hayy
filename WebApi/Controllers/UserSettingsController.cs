using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Authorize] // لازم يكون عامل Login
    [ApiController]
    [Route("api/[controller]")]
    public class UserSettingsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserSettingsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // 1. جلب الإعدادات الحالية عشان تظهر على الـ UI
        [HttpGet]
        public async Task<IActionResult> GetMySettings()
        {
            // سحب الـ ID من الـ Token
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var settings = await _unitOfWork.GetRepository<UserSettings>()
                .GetAsync(s => s.UserId == userId);

            if (settings == null) return NotFound("Settings not found.");

            return Ok(settings);
        }

        // 2. تحديث الإعدادات (لما يدوس Toggle)
        [HttpPut]
        public async Task<IActionResult> UpdateSettings(UpdateUserSettingsDTO dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var settings = await _unitOfWork.GetRepository<UserSettings>()
                .GetAsync(s => s.UserId == userId);

            if (settings == null) return NotFound();

            // تحديث القيم
            settings.EmailNotifications = dto.EmailNotifications;
            settings.NotificationsEnabled = dto.NotificationsEnabled;

            _unitOfWork.GetRepository<UserSettings>().Update(settings);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "تم تحديث الإعدادات بنجاح" });
        }
    }
}
