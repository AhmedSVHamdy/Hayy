using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Core.DTO.Plans;
using Project.Core.ServiceContracts;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionPlansController : ControllerBase
    {
        private readonly ISubscriptionPlanService _service;

        public SubscriptionPlansController(ISubscriptionPlanService service)
        {
            _service = service;
        }

        // ===========================
        // GET api/subscriptionplans
        // ===========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var plans = await _service.GetAllAsync();
            return Ok(plans);
        }

        // ===========================
        // GET api/subscriptionplans/{id}
        // ===========================
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var plan = await _service.GetByIdAsync(id);

            if (plan is null)
                return NotFound(new { Message = $"Plan with id '{id}' not found or is inactive." });

            return Ok(plan);
        }

        // ===========================
        // POST api/subscriptionplans
        // ===========================
        [HttpPost]
        public async Task<IActionResult> AddPlan([FromBody] AddSubscriptionPlanRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _service.AddPlanAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created
            );
        }
        // ===========================
        // PUT api/subscriptionplans/{id}
        // ===========================
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] UpdateSubscriptionPlanRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdatePlanAsync(id, dto);

            if (updated is null)
                return NotFound(new { Message = $"Plan with id '{id}' not found or is inactive." });

            return Ok(updated);
        }

        // ===========================
        // DELETE api/subscriptionplans/{id}  → Soft Delete
        // ===========================
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePlan(Guid id)
        {
            var result = await _service.DeletePlanAsync(id);

            if (!result)
                return NotFound(new { Message = $"Plan with id '{id}' not found or already inactive." });

            return NoContent(); // 204 ✅
        }
    }
}
