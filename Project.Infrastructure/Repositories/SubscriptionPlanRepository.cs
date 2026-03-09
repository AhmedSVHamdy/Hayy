using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class SubscriptionPlanRepository : ISubscriptionPlanRepository
    {
        private readonly HayyContext _context;

        public SubscriptionPlanRepository(HayyContext context)
        {
            _context = context;
        }

        // ===== من الـ Generic =====

        public async Task<IEnumerable<SubscriptionPlan>> GetAllAsync()
            => await _context.SubscriptionPlans
                .AsNoTracking()
                .ToListAsync();

        public async Task<SubscriptionPlan?> GetByIdAsync(Guid id)
            => await _context.SubscriptionPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task AddAsync(SubscriptionPlan entity)
            => await _context.SubscriptionPlans.AddAsync(entity);

        public void Remove(SubscriptionPlan entity)
            => _context.SubscriptionPlans.Remove(entity);

        public void Update(SubscriptionPlan entity)
            => _context.SubscriptionPlans.Update(entity);

        public async Task<SubscriptionPlan?> GetAsync(Expression<Func<SubscriptionPlan, bool>> predicate)
            => await _context.SubscriptionPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate);

        // ===== Specific - Soft Delete =====

        public void SoftDelete(SubscriptionPlan plan)
        {
            plan.IsActive = false;
            _context.SubscriptionPlans.Update(plan);
        }
    }
}
