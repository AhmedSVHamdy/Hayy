using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
   
        public interface ISubscriptionPlanRepository : IGenericRepository<SubscriptionPlan>
        {
        void SoftDelete(SubscriptionPlan plan); // بس الـ specific
        }

}
