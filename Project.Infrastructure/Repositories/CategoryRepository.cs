using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
       

        public async Task<Category?> GetByIdWithTagsAsync(Guid id)
        {
            return await _context.Categories
                .Include(c => c.CategoryTags)      // هات جدول الربط
                    .ThenInclude(ct => ct.Tag)     // ومنه هات الوسم نفسه
                .FirstOrDefaultAsync(c => c.Id == id);
        }

       
   

  private readonly HayyContext _context;

        public CategoryRepository(HayyContext context) : base(context)
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
