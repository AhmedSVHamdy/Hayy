using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
namespace Project.Infrastructure.Repositories
{
   

    public class PlaceRepository : GenericRepository<Place>, IPlaceRepository
    {
        private readonly HayyContext _context;

        public PlaceRepository(HayyContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Place?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Places
                .Include(p => p.Category)       // هات التصنيف
                .Include(p => p.PlaceTags)      // هات جدول الربط
                    .ThenInclude(pt => pt.Tag)  // ومنه هات الوسم
                .Include(p => p.OpeningHours)   // هات المواعيد
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Place>> GetAllWithDetailsAsync()
        {
            return await _context.Places
                .Include(p => p.Category)
                .Include(p => p.PlaceTags)
                    .ThenInclude(pt => pt.Tag)
                .ToListAsync();
        }
        public async Task UpdatePlaceRatingAsync(Guid placeId)
        {
            // 1. احسب المتوسط والعدد من جدول Reviews مباشرة (Database Side)
            // ده أسرع بكتير من إنك تجيب الليستة كلها في الميموري
            var stats = await _context.Reviews
                .Where(r => r.PlaceId == placeId)
                .GroupBy(r => r.PlaceId)
                .Select(g => new
                {
                    Average = g.Average(r => r.Rating),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync();

            // 2. هات المكان عشان نحدثه
            var place = await _context.Places.FindAsync(placeId);

            if (place != null)
            {
                if (stats != null)
                {
                    // لو فيه ريفيوهات، حدث القيم
                    place.AvgRating = (decimal)stats.Average; // تحويل من double لـ decimal
                    place.TotalReviews = stats.Count;
                }
                else
                {
                    // لو مفيش ريفيوهات (مثلاً آخر ريفيو اتمسح)، صفر العدادات
                    place.AvgRating = 0;
                    place.TotalReviews = 0;
                }

                // 3. احفظ التعديل في جدول Places
                await _context.SaveChangesAsync();
            }
        }
    }
}
