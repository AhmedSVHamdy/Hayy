using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RopositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly HayyContext _context;

        public CategoryRepository(HayyContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllWithTagsAsync()
        {
            // هنا الكود المعقد بتاع الداتابيز
            return await _context.Categories
                .Include(c => c.CategoryTags)
                .ThenInclude(ct => ct.Tag)
                .ToListAsync();
        }

        public async Task<List<Guid>> GetTopCategoryIdsAsync(int count)
        {
            return await _context.Categories
                .Take(count)
                .Select(c => c.Id)
                .ToListAsync();
        }
    }
}
