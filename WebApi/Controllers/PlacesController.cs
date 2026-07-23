using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO.Places;
using Project.Core.ServiceContracts;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/Places")]
    public class PlacesController : ControllerBase
    {
        private readonly IPlaceService _placeService;
        private readonly IBusinessRepository _businessRepo;

        public PlacesController(IPlaceService placeService, IBusinessRepository businessRepo)
        {
            _placeService = placeService;
            _businessRepo = businessRepo;
        }

        /// <summary>
        /// Create a new place 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Business")]
        [Consumes("multipart/form-data")] // ✅ عشان يقبل الصور
        public async Task<IActionResult> Create([FromForm] CreatePlaceDto dto) // ✅ FromForm مش FromBody
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
            }
        }

        /// <summary>
        /// get place by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var place = await _placeService.GetPlaceByIdAsync(id);
            if (place == null) return NotFound("المكان غير موجود");
            return Ok(place);
        }

        /// <summary>
        /// Get all places 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var places = await _placeService.GetAllPlacesAsync();
            return Ok(places);
        }

        /// <summary>
        /// Get places by category id
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetPlacesByCategoryIdAsync(Guid categoryId)
        {
            var places = await _placeService.GetPlacesByCategoryIdAsync(categoryId);

            if (places == null || !places.Any())
                return NotFound($"لا توجد أماكن مرتبطة بالتصنيف المعرف بـ: {categoryId}");

            return Ok(places);
        }

        /// <summary>
        /// Get places for the authenticated business user
        /// </summary>
        /// <returns></returns>
        [HttpGet("my-places")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> GetMyPlaces()
        {
            var businessId = await GetBusinessIdAsync();
            if (businessId == null)
                return Unauthorized("مش قادر أحدد هوية البيزنس");

            var places = await _placeService.GetPlacesByBusinessAsync(businessId.Value);
            return Ok(places);
        }

        /// <summary>
        /// Update a place (only for the owner of the place)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Business")]
        [Consumes("multipart/form-data")] // ✅ عشان يقبل الصور
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdatePlaceDto dto) // ✅ FromForm
        {
            var businessId = await GetBusinessIdAsync();
            if (businessId == null)
                return Unauthorized("مش قادر أحدد هوية البيزنس");

            try
            {
                var result = await _placeService.UpdatePlaceAsync(id, businessId.Value, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a place (only for the owner of the place)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var businessId = await GetBusinessIdAsync();
            if (businessId == null)
                return Unauthorized("مش قادر أحدد هوية البيزنس");

            try
            {
                await _placeService.DeletePlaceAsync(id, businessId.Value);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ===================== Helper =====================
        private async Task<Guid?> GetBusinessIdAsync()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(claim, out var userId)) return null;

            var business = await _businessRepo.GetBusinessByUserIdAsync(userId);
            return business?.Id;
        }
    }
}