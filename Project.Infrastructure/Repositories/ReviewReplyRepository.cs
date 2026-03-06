using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class ReviewReplyRepository : IReviewReplyRepository
    {
        private readonly HayyContext _context;

        public ReviewReplyRepository(HayyContext context)
        {
            _context = context;
        }

        public async Task<ReviewReply> AddAsync(ReviewReply reply)
        {
            await _context.ReviewReplies.AddAsync(reply);
            await _context.SaveChangesAsync();
            return reply;
        }

        public async Task<ReviewReply?> GetByIdAsync(Guid id)
        {
            return await _context.ReviewReplies.FindAsync(id);
        }

        public async Task<List<ReviewReply>> GetRepliesByReviewIdPagedAsync(Guid reviewId, int pageNumber, int pageSize)
        {
            return await _context.ReviewReplies
                .Where(r => r.ReviewId == reviewId)
                .OrderByDescending(r => r.CreatedAt) // الأحدث الأول
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountByReviewIdAsync(Guid reviewId)
        {
            return await _context.ReviewReplies.CountAsync(r => r.ReviewId == reviewId);
        }

        public async Task DeleteAsync(ReviewReply reply)
        {
            _context.ReviewReplies.Remove(reply);
            await _context.SaveChangesAsync();
        }
        
    }
}
