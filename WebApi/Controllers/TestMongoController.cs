//using Microsoft.AspNetCore.Mvc;
//using Project.Core.Domain.Entities;
//using Project.Core.Domain.RepositoryContracts;
//using Project.Core.Enums;

//namespace WebApi.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class TestMongoController : ControllerBase
//    {
//        private readonly IRecommendedItemRepository _repo;

//        public TestMongoController(IRecommendedItemRepository repo)
//        {
//            _repo = repo;
//        }

//        [HttpPost("test-recommendation")]
//        public async Task<IActionResult> Test()
//        {
//            var testItem = new RecommendedItem
//            {
//                Id = Guid.NewGuid(),
//                UserId = Guid.NewGuid(), // أي Guid للتجربة
//                ItemType = ItemType.Post, // أو أي قيمة عندك في الـ Enum
//                ItemId = Guid.NewGuid(),
//                Score = 0.95m,
//                CreatedAt = DateTime.UtcNow,
//                UpdatedAt = DateTime.UtcNow
//            };

//            await _repo.CreateAsync(testItem);
//            return Ok("تم الإرسال للمنجو بنجاح! روح شيك على الأطلس دلوقتي.");
//        }
//    }
//}
