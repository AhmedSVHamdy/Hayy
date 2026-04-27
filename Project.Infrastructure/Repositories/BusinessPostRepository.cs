using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class BusinessPostRepository : IBusinessPostRepository
    {
        private readonly HayyContext _context;

        public BusinessPostRepository(HayyContext context)
        {
            _context = context;
        }

        public async Task<BusinessPost> AddPostAsync(BusinessPost post)
        {
            _context.BusinessPosts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<int> GetCountByPlaceIdAsync(Guid placeId)
        {
            return await _context.BusinessPosts
                .CountAsync(p => p.PlaceId == placeId);
        }

        public async Task<BusinessPost?> GetPostByIdAsync(Guid postId)
        {
            return await _context.BusinessPosts
        .Include(p => p.Place)
        .ThenInclude(p => p.PlaceTags) // 👈 وهات التاجز المربوطة بالمكان
        .FirstOrDefaultAsync(p => p.Id == postId);
        }

        public async Task<IEnumerable<BusinessPost>> GetPostsByPlaceIdAsync(Guid placeId)
        {
            return await _context.BusinessPosts
                .Where(p => p.PlaceId == placeId)
                .Include(p => p.Place)       // عشان نجيب اسم وصورة المطعم
                .Include(p => p.PostLikes)   // عشان نعد اللايكات
                .Include(p => p.PostComments)// عشان نعد الكومنتات
                .OrderByDescending(p => p.CreatedAt) // الأحدث الأول
                .ToListAsync();
        }

        public async Task<List<BusinessPost>> GetPostsByPlaceIdPagedAsync(Guid placeId, int pageNumber, int pageSize)
        {
            return await _context.BusinessPosts
                .Where(p => p.PlaceId == placeId)
                .OrderByDescending(p => p.CreatedAt) // الأحدث يظهر الأول
                .Skip((pageNumber - 1) * pageSize)   // تفويت الصفحات السابقة
                .Take(pageSize)                      // جلب العدد المطلوب
                .ToListAsync();
        }
        public async Task UpdateAsync(BusinessPost post)
        {
            _context.BusinessPosts.Update(post);
            await _context.SaveChangesAsync(); // بنحفظ التعديلات في الداتابيز
        }

        // 3. دالة الحذف
        public async Task DeleteAsync(BusinessPost post)
        {
            _context.BusinessPosts.Remove(post);
            await _context.SaveChangesAsync(); // بنحفظ الحذف في الداتابيز
        }

        public async Task<List<BusinessPost>> GetAllPostsPagedAsync(int pageNumber, int pageSize)
        {
            return await _context.BusinessPosts
                .Include(p => p.Place)
                .Include(p => p.PostLikes)
                .Include(p => p.PostComments)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.BusinessPosts.CountAsync();
        }
    }
}
