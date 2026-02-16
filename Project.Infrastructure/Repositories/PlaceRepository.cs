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
    }
}
