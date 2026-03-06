using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly HayyContext _context;

        public ReviewRepository(HayyContext context)
        {
            _context = context;
        }

        public async Task<Review> AddReviewAsync(Review review)
        {
            // إضافة للصف في الذاكرة
            await _context.Reviews.AddAsync(review);

            // الحفظ الفعلي في SQL Server
            await _context.SaveChangesAsync();

            return review;
        }
        public async Task<IEnumerable<Review>> GetReviewsByPlaceIdAsync(Guid placeId)
        {
            return await _context.Reviews
                .Where(r => r.PlaceId == placeId)
                .Include(r => r.User) // عشان نعرض اسم اليوزر اللي عمل التقييم
                .OrderByDescending(r => r.CreatedAt) // الأحدث الأول
                .ToListAsync();
        }

     
        public async Task<List<Review>> GetReviewsPagedAsync(Guid placeId, int pageNumber, int pageSize)
        {
            return await _context.Reviews
                .Where(r => r.PlaceId == placeId)      // فلتر بالمكان
                .Include(r => r.User)                  // 👈 مهم جداً: هات بيانات اليوزر (الاسم والصورة) مع الريفيو
                .OrderByDescending(r => r.CreatedAt)   // الأحدث الأول
                .Skip((pageNumber - 1) * pageSize)     // فوت الصفحات اللي فاتت
                .Take(pageSize)                        // هات العدد المطلوب بس
                .ToListAsync();
        }

        // 👇 2. تنفيذ دالة عد الريفيوهات
        public async Task<int> GetCountByPlaceIdAsync(Guid placeId)
        {
            return await _context.Reviews
                .CountAsync(r => r.PlaceId == placeId);
        }

        // 👇 3. دالة التأكد من عدم التكرار (لو مش مكتوبة عندك)
        public async Task<bool> HasUserReviewedPlaceAsync(Guid userId, Guid placeId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.PlaceId == placeId);
        }
        public async Task<Review?> GetReviewByIdAsync(Guid id)
        {
            return await _context.Reviews
                .Include(r => r.User) // عشان تجيب بيانات صاحب الريفيو للإشعار
                .Include(r => r.Place)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
