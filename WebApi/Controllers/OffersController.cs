using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.ServiceContracts;
using static Project.Core.DTO.CreateOfferDTO;
using Microsoft.AspNetCore.Http;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OffersController : ControllerBase
    {
        private readonly IOfferService _offerService;

        public OffersController(IOfferService offerService)
        {
            _offerService = offerService;
        }
        /// <summary>
        /// Create a new offer. This endpoint is accessible only to users with the "Business" role.
        /// </summary>
        /// <param name="dto">The offer data to be created.</param>
        /// <returns>The created offer.</returns>
        [HttpPost]
        [Authorize(Roles = "Business")]
        // 👈 التعديل الوحيد هنا: FromForm بدل FromBody
        public async Task<IActionResult> CreateOffer([FromForm] CreateOfferDto dto)
        {
            try
            {
                var result = await _offerService.CreateOfferAsync(dto);
                return CreatedAtAction(nameof(GetOffersByPlaceId), new { placeId = result.PlaceId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        /// <summary>
        /// Get offers by place ID. This endpoint is accessible to all users.
        /// </summary>
        /// <param name="placeId">The ID of the place to get offers for.</param>
        /// <returns>A list of offers for the specified place.</returns>
        [HttpGet("place/{placeId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOffersByPlaceId(Guid placeId)
        {
            var offers = await _offerService.GetOffersByPlaceIdAsync(placeId);
            return Ok(offers);
        }
        /// <summary>
        /// Get offer by ID. This endpoint is accessible to all users.
        /// </summary>
        /// <param name="offerId">The ID of the offer to get.</param>
        /// <returns>The offer with the specified ID.</returns>
        [HttpGet("offer/{offerId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOfferById(Guid offerId)
        {
            try
            {
                var offer = await _offerService.GetOfferByIdAsync(offerId);

                if (offer == null)
                    return NotFound(new { message = "العرض غير موجود" });

                return Ok(offer);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Update an existing offer. This endpoint is accessible only to users with the "Business" role.
        /// </summary>
        /// <param name="id">The ID of the offer to update.</param>
        /// <param name="dto">The updated offer data.</param>
        /// <returns>The updated offer.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Business")]
        // 👈 التعديل هنا كمان: FromForm بدل FromBody
        public async Task<IActionResult> UpdateOffer(Guid id, [FromForm] UpdateOfferDto dto)
        {
            try
            {
                // ضمان إن الـ ID اللي في الرابط هو اللي هيتعدل
                dto.Id = id;
                var result = await _offerService.UpdateOfferAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        /// <summary>
        /// Delete an existing offer. This endpoint is accessible only to users with the "Business" role.
        /// </summary>
        /// <param name="id">The ID of the offer to delete.</param>
        /// <returns>No content if the deletion is successful.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> DeleteOffer(Guid id)
        {
            try
            {
                await _offerService.DeleteOfferAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        /// <summary>
        /// Get all active offers. This endpoint is accessible to all users.
        /// </summary>
        /// <returns>A list of all active offers.</returns>
        [HttpGet("active")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetActiveOffers()
        {
            try
            {
                var offers = await _offerService.GetActiveOffersAsync();

                // التحقق مما إذا كانت القائمة فارغة
                if (offers == null || !offers.Any())
                {
                    return NotFound(new { Message = "لا توجد عروض متاحه الان" });
                }

                return Ok(offers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}