using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IUnitOfWork : IDisposable
    {
        ISubscriptionPlanRepository SubscriptionPlans { get; }

        IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        IPaymentRepository Payments { get; }
       // ISubscriptionPlanRepository SubscriptionPlan { get; }
        IBusinessSubscriptionRepository BusinessSubscriptions { get; }
        //IGenericRepository<SubscriptionPlan> SubscriptionPlans { get; } // ده Generic كافي لأنه بسيط
        IEventBookingRepository EventBookings { get; }

        //Task<int> CompleteAsync();
        Task<int> SaveChangesAsync();
    }
}
