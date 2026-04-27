using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;

[Route("api/search")]
[ApiController]
public class SearchController : ControllerBase
{
    private readonly IUserLogService _userLogService;
    private readonly IPlaceService _placeService; // 👈 هنستخدم دي بس مؤقتاً
    private readonly IUserInterestRepository _interestRepository;

    public SearchController(IUserLogService userLogService, IPlaceService placeService , IUserInterestRepository interestRepository)
    {
        _userLogService = userLogService;
        _placeService = placeService;
        _interestRepository = interestRepository;
    }
    /// <summary>
    /// Performs a smart search for places based on the specified search criteria and returns the matching results.
    /// </summary>
    /// <remarks>This method logs the search activity before performing the search. The search is currently
    /// performed using a basic SQL query until the AI model is available. The returned results can be filtered by
    /// category and tag if provided.</remarks>
    /// <param name="request">An object containing the search term and optional filters such as category and tag identifiers. The search term
    /// must not be null, empty, or whitespace.</param>
    /// <returns>An <see cref="IActionResult"/> containing the search results if the operation is successful; otherwise, a bad
    /// request response if the search term is missing, or an internal server error response if an unexpected error
    /// occurs.</returns>
    [HttpPost("smart-search")]
    public async Task<IActionResult> SmartSearch([FromBody] LogSearchRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
            return BadRequest("يجب إدخال كلمة للبحث");

        // ==========================================
        // 🌟 1. جلب اهتمامات اليوزر الحالية (Snapshot)
        // ==========================================
        Guid currentUserId = request.UserId ?? Guid.Empty;
        Guid? userTopCategoryId = null;
        List<Guid> userTagIds = new List<Guid>();

        // لو اليوزر مسجل دخول (مش Guest)، هات اهتماماته
        if (currentUserId != Guid.Empty)
        {
            var userInterests = await _interestRepository.GetUserInterestsByUserIdAsync(currentUserId);

            userTopCategoryId = userInterests
                .OrderByDescending(i => i.InterestScore)
                .FirstOrDefault(i => i.CategoryId.HasValue)?.CategoryId;

            userTagIds = userInterests
                .Where(i => i.TagId.HasValue)
                .Select(i => i.TagId.Value)
                .ToList();
        }
        var logDto = new CreateUserLogDto
        {
            UserId = currentUserId,
            ActionType = ActionType.Search,
            TargetType = TargetType.Place,
            SearchQuery = request.SearchTerm.Trim().ToLower(),
            CategoryId = request.CategoryId,
            TagId = request.TagId ?? new List<Guid>(),
            Details = "Find a place",

            // 👇 الحقول الجديدة اللي ضفناها للـ AI
            UserTopInterestCategoryId = userTopCategoryId,
            UserInterestTagIds = userTagIds
        };

        await _userLogService.LogActivityAsync(logDto);

        // ==========================================
        // 🚧 كود مؤقت لحد ما موديل الـ AI يخلص 🚧
        // ==========================================
        try
        {
            // هنروح ندور في الـ SQL بالكلمة اللي اليوزر كتبها مباشرة (بحث تقليدي)
            var places = await _placeService.BasicSearchAsync(request.SearchTerm, request.CategoryId);

            // نرجع النتيجة للفلاتر
            return Ok(places);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "حدث خطأ أثناء البحث");
        }
    }
}