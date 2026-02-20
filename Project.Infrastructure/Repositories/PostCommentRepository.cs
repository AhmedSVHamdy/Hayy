using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class PostCommentRepository : IPostCommentRepository
    {
        private readonly HayyContext _context;
        public PostCommentRepository(HayyContext context)
        {
            _context = context;
        }
        public async Task<PostComment> AddCommentAsync(PostComment comment)
        {
            _context.PostComments.Add(comment);
           await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<PostComment?> GetCommentByIdAsync(Guid commentId)
        {
            return await _context.PostComments
            .FirstOrDefaultAsync(c => c.Id == commentId);
        }

        public async Task<IEnumerable<PostComment>> GetCommentsByPostIdAsync(Guid postId)
        {
            // بنجيب الكومنتات اللي ملهاش أب (ParentCommentId == null) يعني الرئيسية
            // وبنعمل Include للردود (Replies) واليوزر
            return await _context.PostComments
                .Where(c => c.PostId == postId && c.ParentCommentId == null)
                .Include(c => c.User) // بيانات صاحب الكومنت الأصلي
                .Include(c => c.Replies) // الردود
                    .ThenInclude(r => r.User) // بيانات أصحاب الردود
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PostComment>> GetCommentsByPostIdPagedAsync(Guid postId, int pageNumber, int pageSize)
        {
            return await _context.PostComments
                .Where(c => c.PostId == postId)
                .Include(c => c.User) // ✅ مهم جداً: عشان نعرض اسم وصورة اللي كتب الكومنت
                .OrderByDescending(c => c.CreatedAt) // الأحدث أولاً
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCommentsCountAsync(Guid postId)
        {
            return await _context.PostComments.CountAsync(c => c.PostId == postId);
        }
    }
}
