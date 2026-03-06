using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;

[Route("api/search")]
[ApiController]
public class SearchController : ControllerBase
{
    private readonly IUserLogService _userLogService;
    private readonly IPlaceService _placeService; // 👈 هنستخدم دي بس مؤقتاً

    public SearchController(IUserLogService userLogService, IPlaceService placeService)
    {
        _userLogService = userLogService;
        _placeService = placeService;
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

        // 1. تسجيل البحث في المونجو (شغال 10/10)
        var logDto = new CreateUserLogDto
        {
            UserId = request.UserId ?? Guid.Empty,
            ActionType = ActionType.Search,
            TargetType = TargetType.Place,
            SearchQuery = request.SearchTerm.Trim().ToLower(),
            CategoryId = request.CategoryId,
            TagId = request.TagId ?? new List<Guid>(),
            Details = "Find a place"
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