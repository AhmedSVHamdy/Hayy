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

        public async Task<BusinessPost?> GetPostByIdAsync(Guid postId)
        {
            return await _context.BusinessPosts
        .Include(p => p.Place)
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
    }
}
