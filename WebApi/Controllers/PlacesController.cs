using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain.Entities;
using Project.Core.DTO.Places;
using Project.Core.Helpers;
using Project.Core.ServiceContracts;

namespace WebApi.Controllers
{
   
    [ApiController]
    [Route("api/Places")]
    public class PlacesController : ControllerBase
    {
        private readonly IPlaceService _placeService;

        public PlacesController(IPlaceService placeService)
        {
            _placeService = placeService;
        }
        /// <summary>
        /// Creates a new place using the specified data.
        /// </summary>
        /// <remarks>This action is restricted to users in the "Business" role. The request body must
        /// contain valid place information. If the creation is successful, the response includes a location header
        /// pointing to the newly created resource.</remarks>
        /// <param name="dto">The data transfer object containing the details of the place to create. Must not be null and must satisfy
        /// all validation requirements.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the create operation. Returns a 201 Created
        /// response with the created place if successful; otherwise, returns a 400 Bad Request with validation or error
        /// details.</returns>
        [HttpPost]
        [Authorize(Roles = "Business")] // لازم يكون صاحب مكان
        public async Task<IActionResult> Create([FromBody] CreatePlaceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _placeService.CreatePlaceAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message = "فشل الحفظ", error = innerMessage });
               // return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Retrieves the details of a place by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the place to retrieve.</param>
        /// <returns>An <see cref="IActionResult"/> containing the place details if found; otherwise, a NotFound result if the
        /// place does not exist.</returns>
        [HttpGet("{id}")]
        [Authorize] 
        public async Task<IActionResult> GetById(Guid id)
        {
            var place = await _placeService.GetPlaceByIdAsync(id);
            if (place == null) return NotFound("المكان غير موجود");
            return Ok(place);
        }
        /// <summary>
        /// Retrieves a list of all places accessible to the current user.
        /// </summary>
        /// <remarks>This action requires the caller to be authenticated and authorized with the "User"
        /// role. Only users with the appropriate role can access the list of places.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a collection of place objects. The response has a status code of
        /// 200 (OK) with the list of places if successful.</returns>
        [HttpGet]
        [Authorize(Roles = "User")] 
        public async Task<IActionResult> GetAll()
        {
            var places = await _placeService.GetAllPlacesAsync();
            return Ok(places);
        }

        [HttpGet("category/{categoryId}")] 
        public async Task<IActionResult> GetPlacesByCategoryIdAsync(Guid categoryId)
        {
            // 1. استدعاء الدالة من الـ Service
            var places = await _placeService.GetPlacesByCategoryIdAsync(categoryId);

            // 2. التحقق لو مفيش أماكن (اختياري: ممكن ترجع قائمة فاضية أو NotFound)
            if (places == null || !places.Any())
            {
                return NotFound($"لا توجد أماكن مرتبطة بالتصنيف المعرف بـ: {categoryId}");
            }

            // 3. إرجاع البيانات بنجاح
            return Ok(places);
        }
    }
}
