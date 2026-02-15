using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class PostLikeRepository : IPostLikeRepository
    {
        private readonly HayyContext _context;
        public PostLikeRepository(HayyContext context)
        {
            _context = context;
        }

        public async Task AddLikeAsync(PostLike like)
        {
            _context.PostLikes.Add(like);
            await _context.SaveChangesAsync();
        }

        public async Task<PostLike?> GetLikeAsync(Guid userId, Guid postId)
        {
            return await _context.PostLikes
                .FirstOrDefaultAsync(x => x.UserId == userId && x.PostId == postId);
        }

        public async Task<int> GetLikesCountAsync(Guid postId)
        {
            return await _context.PostLikes.CountAsync(x => x.PostId == postId);
        }

        public async Task RemoveLikeAsync(PostLike like)
        {
            _context.PostLikes.Remove(like);
            await _context.SaveChangesAsync();
        }
    }
}
