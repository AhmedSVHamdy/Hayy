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

        public async Task<bool> HasUserReviewedPlaceAsync(Guid userId, Guid placeId)
        {
            // بنقول للداتابيز: هل فيه أي ريفيو بيحقق الشرطين دول مع بعض؟
            return await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.PlaceId == placeId);
        }
    }
}
