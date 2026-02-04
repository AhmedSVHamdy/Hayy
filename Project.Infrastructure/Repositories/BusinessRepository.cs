using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RopositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class BusinessRepository : IBusinessRepository
    {
        private readonly HayyContext _context;

        public BusinessRepository(HayyContext context)
        {
            _context = context;
        }

        public async Task<Business?> GetByIdAsync(Guid id)
        {
            // بنبحث عن البيزنس
            // ملحوظة: لو محتاج بيانات اليوزر معاه ممكن تزود .Include(b => b.User)
            return await _context.Businesses
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task UpdateAsync(Business business)
        {
            _context.Businesses.Update(business);
            await _context.SaveChangesAsync();
        }
    }
}
