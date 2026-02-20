using System;
using System.Collections.Generic;
using System.Text;

using Project.Core.Domain.Entities;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IBusinessSubscriptionRepository : IGenericRepository<BusinessSubscription>
    {
        // دالة بتجيب الاشتراك الساري حالياً (لو موجود)
        Task<BusinessSubscription?> GetActiveSubscriptionAsync(Guid businessId);
    }
}
