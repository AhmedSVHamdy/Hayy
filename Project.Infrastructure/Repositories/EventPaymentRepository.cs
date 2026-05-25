using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class EventPaymentRepository : GenericRepository<EventPayment>, IEventPaymentRepository
    {
        // 1. ضفنا متغير هنا عشان نمسك بيه الـ DbContext
        private readonly HayyContext _context;

        public EventPaymentRepository(HayyContext dbContext) : base(dbContext)
        {
            // 2. سوينا المتغير بالـ dbContext اللي جاي من الـ Constructor
            _context = dbContext;
        }

        public async Task<EventPayment?> GetByPaymobOrderIdAsync(long paymobOrderId)
        {
            // 3. استخدمنا _context.EventPayments بدل _dbSet
            return await _context.EventPayments
                                 .FirstOrDefaultAsync(p => p.PaymobOrderId == paymobOrderId);
        }
    }
}
