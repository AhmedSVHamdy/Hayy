using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IEventPaymentRepository : IGenericRepository<EventPayment>
    {
        Task<EventPayment?> GetByPaymobOrderIdAsync(long paymobOrderId);
    }
}
