using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        private readonly HayyContext _context;

        public PaymentRepository(HayyContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Payment?> GetByPaymobOrderIdAsync(long paymobOrderId)
        {
            return await _context.Payments
                                 .FirstOrDefaultAsync(p => p.PaymobOrderId == paymobOrderId);
        }
    }
}
