using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IBusinessRepository
    {


        Task AddBusinessAsync(Business business); // وحدنا الاسم ليكون AddBusinessAsync
        Task<Business?> GetBusinessByIdAsync(Guid id);
        Task<Business?> GetBusinessByUserIdAsync(Guid userId); // 👈 هذا هو سبب الخطأ، لازم يكون موجود هنا
        Task UpdateBusinessAsync(Business business);

        // 2. إدارة التوثيق
        Task AddVerificationAsync(BusinessVerification verification);
        Task UpdateVerificationAsync(BusinessVerification verification);
        Task<BusinessVerification?> GetLatestVerificationByBusinessIdAsync(Guid businessId);

        // 3. لوحة تحكم الأدمن
        Task<List<Business>> GetPendingVerificationsAsync();

    }
}
