using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class PlaceFollowRepository : IPlaceFollowRepository
    {
        private readonly HayyContext _context;

        public PlaceFollowRepository(HayyContext context)
        {
            _context = context;
        }

        public async Task<PlaceFollow?> GetFollowAsync(Guid userId, Guid placeId)
        {
            return await _context.PlaceFollows.FirstOrDefaultAsync(f => f.UserId == userId && f.PlaceId == placeId);
        }


        public async Task AddAsync(PlaceFollow placeFollow) 
        {
            await _context.PlaceFollows.AddAsync(placeFollow);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(PlaceFollow placeFollow)
        {
             _context.PlaceFollows.Remove(placeFollow);
            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<PlaceFollow> Items, int TotalCount)> GetFollowersByPlaceIdAsync(Guid placeId, int page, int size)
        {
            var query = _context.PlaceFollows.Include(f => f.User).Where(f => f.PlaceId == placeId);
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            return (items, totalCount);
        }

        public async Task<(IEnumerable<PlaceFollow> Items, int TotalCount)> GetFollowedPlacesByUserIdAsync(Guid userId, int page, int size)
        {
            var query = _context.PlaceFollows.Include(f => f.Place).Where(f => f.UserId == userId);
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            return (items, totalCount);
        }
    }
}
