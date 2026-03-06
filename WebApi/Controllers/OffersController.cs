using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.ServiceContracts;
using static Project.Core.DTO.CreateOfferDTO;

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

        // 🔒 أصحاب الأماكن بس هما اللي يقدروا يعملوا عروض
        /// <summary>
        /// Creates a new offer for the authenticated business.
        /// </summary>
        /// <remarks>Only users with the 'Business' role are authorized to create offers using this
        /// endpoint.</remarks>
        /// <param name="dto">The data transfer object containing the details of the offer to create. Must not be null.</param>
        /// <returns>An HTTP 201 Created response containing the created offer if successful; otherwise, an HTTP 400 Bad Request
        /// response with error details.</returns>
        [HttpPost]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> CreateOffer([FromBody] CreateOfferDto dto)
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

        // 🌍 مفتوحة لأي يوزر يشوف العروض
        /// <summary>
        /// Retrieves all offers associated with the specified place identifier.
        /// </summary>
        /// <param name="placeId">The unique identifier of the place for which to retrieve offers.</param>
        /// <returns>An <see cref="IActionResult"/> containing a collection of offers for the specified place. Returns an empty
        /// collection if no offers are found.</returns>
        [HttpGet("place/{placeId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOffersByPlaceId(Guid placeId)
        {
            var offers = await _offerService.GetOffersByPlaceIdAsync(placeId);
            return Ok(offers);
        }
        // 🔒 التعديل - لأصحاب الأماكن بس
        /// <summary>
        /// Updates an existing offer with the specified identifier using the provided data.
        /// </summary>
        /// <remarks>This action is restricted to users with the "Business" role. The offer identifier in
        /// the route must match the offer being updated.</remarks>
        /// <param name="id">The unique identifier of the offer to update.</param>
        /// <param name="dto">An object containing the updated offer details. The object's Id property is set to match the specified offer
        /// identifier.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the update operation. Returns 200 OK with the
        /// updated offer if successful; 404 Not Found if the offer does not exist; or 400 Bad Request if the update
        /// fails due to invalid input.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> UpdateOffer(Guid id, [FromBody] UpdateOfferDto dto)
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

        // 🔒 الحذف - لأصحاب الأماكن بس
        /// <summary>
        /// Deletes the offer with the specified identifier.
        /// </summary>
        /// <remarks>This action is restricted to users with the "Business" role. Only authorized business
        /// users can delete offers.</remarks>
        /// <param name="id">The unique identifier of the offer to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the delete operation. Returns 204 No Content if the
        /// offer was successfully deleted; 404 Not Found if the offer does not exist; or 400 Bad Request for other
        /// errors.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Business")]
        public async Task<IActionResult> DeleteOffer(Guid id)
        {
            try
            {
                await _offerService.DeleteOfferAsync(id);
                return NoContent(); // 204 No Content (الرد القياسي لنجاح الحذف)
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
    }
}
