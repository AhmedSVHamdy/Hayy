using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Core.DTO.Categories;
using Project.Core.DTO.Tags;
using Project.Core.ServiceContracts;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        
        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        /// <summary>
        /// Creates a new category using the specified data transfer object.
        /// </summary>
        /// <param name="dto">The data transfer object containing the details of the category to create. Cannot be null.</param>
        /// <returns>An IActionResult that represents the result of the create operation. Returns a 200 OK response with the
        /// created category data if successful.</returns>
        [HttpPost]

        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            var result = await _categoryService.CreateCategoryAsync(dto);
            return Ok(result);
        }
        /// <summary>
        /// Retrieves the category with the specified unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the category to retrieve.</param>
        /// <returns>An <see cref="IActionResult"/> containing the category data if found; otherwise, a NotFound result.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // Endpoint لربط الوسوم بالتصنيف
        /// <summary>
        /// Assigns one or more tags to a specified category.
        /// </summary>
        /// <param name="dto">An object containing the category identifier and the list of tag identifiers to assign. Cannot be null.</param>
        /// <returns>An HTTP 200 response if the tags are assigned successfully; otherwise, an HTTP 400 response with an error
        /// message.</returns>
        [HttpPost("assign-tags")]
        public async Task<IActionResult> AssignTags([FromBody] AssignTagsDto dto)
        {
            try
            {
                await _categoryService.AssignTagsToCategoryAsync(dto);
                return Ok(new { Message = "تم ربط الوسوم بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
