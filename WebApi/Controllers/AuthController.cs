using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // عشان نستخدم EF Core
 // عشان يشوف IImageService
using Project.Core.Domain.Entities; // عشان يشوف كلاس User (تأكد من المسار)
using Project.Core.DTO;       // عشان يشوف RegisterDto
using Project.Core.ServiceContracts;
using Project.Infrastructure.ApplicationDbContext; // عشان يشوف ApplicationDbContext (تأكد من المسار)





//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
    //    private readonly HayyContext _context; // التعامل مع الداتابيز
    //    private readonly IImageService _imageService;   // التعامل مع صور Azure

    //    // الـ Constructor: هنا بنستلم الأدوات اللي محتاجينها (حقن التبعية)
    //    public AuthController(HayyContext context, IImageService imageService)
    //    {
    //        _context = context;
    //        _imageService = imageService;
    //    }

    //    [HttpPost("register")]
    //    public async Task<IActionResult> Register([FromForm] RegisterDto dto, IFormFile? image)
    //    {
    //        // 1. التأكد إن البيانات سليمة
    //        if (!ModelState.IsValid)
    //            return BadRequest(ModelState);

    //        // 2. رفع الصورة (لو اليوزر باعت صورة)
    //        string imageUrl = null;
    //        if (image != null && image.Length > 0)
    //        {
    //            // هنا السحر بيحصل: السيرفس هترفع الصورة وترجعلنا اللينك جاهز
    //            imageUrl = await _imageService.UploadImageAsync(image);
    //        }

    //        // 3. تجهيز اليوزر للحفظ في الداتابيز
    //        // (غير User حسب اسم الكلاس عندك في الـ Entities)
    //        var user = new User
    //        {
    //            Name = dto.Name,
    //            Email = dto.Email,
    //            Password = dto.Password, // ملحوظة: في الحقيقة بنشفر الباسورد، بس ده للتبسيط حالياً
    //            ProfilePictureUrl = imageUrl // تخزين اللينك اللي جالنا من Azure
    //        };

    //        // 4. الحفظ النهائي
    //        _context.Users.Add(user);
    //        await _context.SaveChangesAsync();

    //        return Ok(new { Message = "تم التسجيل بنجاح!", UserId = user.Id, ImageUrl = imageUrl });
    //    }
    //    // 👇 دالة جديدة عشان تجيب كل اليوزرز وتتأكد إن الداتا مسمعة
    //    [HttpGet("GetAllUsers")]
    //    public async Task<IActionResult> GetAllUsers()
    //    {
    //        // بنجيب كل اليوزرز من الداتابيز
    //        var users = await _context.Users.ToListAsync();

    //        // بنرجعهم في شكل قائمة JSON
    //        return Ok(users);
    //    }
    }
    //public class NotificationsController : ControllerBase
    //{
    //    private readonly INotificationService _notificationService;

    //    public NotificationsController(INotificationService notificationService)
    //    {
    //        _notificationService = notificationService;
    //    }

    //    [HttpPost]
    //    public async Task<IActionResult> Create(NotificationAddRequest request)
    //    {
    //        var result = await _notificationService.CreateNotification(request);
    //        return Ok(result);
    //    }

    //    [HttpGet("{userId}")]
    //    public async Task<IActionResult> GetMyNotifications(Guid userId)
    //    {
    //        var result = await _notificationService.GetMyNotifications(userId);
    //        return Ok(result);
    //    }
    //}
        
    }
