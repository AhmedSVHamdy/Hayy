using System;
using System.Collections.Generic;
using System.Text;

using Project.Core.Domain.Entities;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        // دالة مهمة جداً عشان لما Paymob يرد علينا نعرف دي أنهي عملية
        Task<Payment?> GetByPaymobOrderIdAsync(long paymobOrderId);
    }
}
