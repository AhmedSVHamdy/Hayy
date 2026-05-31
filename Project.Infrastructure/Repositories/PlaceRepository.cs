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

        // للقراءة فقط — مش محتاج Tracking
        public async Task<Place?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Places
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.PlaceTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.OpeningHours)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // للتعديل والحذف — لازم Tracking عشان EF يشوف التغييرات
        public async Task<Place?> GetByIdWithDetailsForUpdateAsync(Guid id)
        {
            return await _context.Places
                .Include(p => p.Category)
                .Include(p => p.PlaceTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.OpeningHours)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Place>> GetAllWithDetailsAsync()
        {
            return await _context.Places
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.PlaceTags)
                    .ThenInclude(pt => pt.Tag)
                .ToListAsync();
        }

        // جيب الأماكن الخاصة ببيزنس معين
        public async Task<IEnumerable<Place>> GetByBusinessIdAsync(Guid businessId)
        {
            return await _context.Places
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.PlaceTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.OpeningHours)
                .Where(p => p.BusinessId == businessId && p.IsActive)
                .ToListAsync();
        }

        public async Task UpdatePlaceRatingAsync(Guid placeId)
        {
            var stats = await _context.Reviews
                .Where(r => r.PlaceId == placeId)
                .GroupBy(r => r.PlaceId)
                .Select(g => new
                {
                    Average = g.Average(r => r.Rating),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync();

            var place = await _context.Places.FindAsync(placeId);

            if (place != null)
            {
                if (stats != null)
                {
                    place.AvgRating = (decimal)stats.Average;
                    place.TotalReviews = stats.Count;
                }
                else
                {
                    place.AvgRating = 0;
                    place.TotalReviews = 0;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Place>> SearchPlacesAsync(string searchTerm, Guid? categoryId)
        {
            var query = _context.Places
                .Include(p => p.Category)
                .Include(p => p.PlaceTags).ThenInclude(pt => pt.Tag)
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value != Guid.Empty)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)) ||
                    p.PlaceTags.Any(pt => pt.Tag.Name.ToLower().Contains(term))
                );
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Place>> GetByCategoryIdAsync(Guid categoryId)
        {
            return await _context.Places
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }
    }
}