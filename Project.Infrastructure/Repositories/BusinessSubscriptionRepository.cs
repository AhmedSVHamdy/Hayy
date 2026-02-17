using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class BusinessSubscriptionRepository : GenericRepository<BusinessSubscription>, IBusinessSubscriptionRepository
    {
        private readonly HayyContext _context;

        public BusinessSubscriptionRepository(HayyContext context) : base(context)
        {
            _context = context;
        }

        public async Task<BusinessSubscription?> GetActiveSubscriptionAsync(Guid businessId)
        {
            // الاشتراك الساري هو اللي تاريخ انتهائه لسه مجاش
            return await _context.BusinessSubscriptions
                                 .Include(s => s.Plan) // هات تفاصيل الباقة معاه
                                 .Where(s => s.BusinessId == businessId && s.EndDate > DateTime.UtcNow && s.IsActive)
                                 .OrderByDescending(s => s.EndDate) // هات أحدث واحد
                                 .FirstOrDefaultAsync();
        }
    }
}
