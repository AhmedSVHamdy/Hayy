using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IUnitOfWork : IDisposable
    {
        

        IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        IPaymentRepository Payments { get; }
        IBusinessSubscriptionRepository BusinessSubscriptions { get; }
        IGenericRepository<SubscriptionPlan> SubscriptionPlans { get; } // ده Generic كافي لأنه بسيط
        IEventBookingRepository EventBookings { get; }

        //Task<int> CompleteAsync();
        Task<int> SaveChangesAsync();
    }
}
