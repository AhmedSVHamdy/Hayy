using Microsoft.AspNetCore.Mvc;
using Project.Core.DTO.Categories;
using Project.Core.DTO.Tags;
using Project.Core.ServiceContracts;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            var result = await _categoryService.CreateCategoryAsync(dto);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // Endpoint لربط الوسوم بالتصنيف
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
