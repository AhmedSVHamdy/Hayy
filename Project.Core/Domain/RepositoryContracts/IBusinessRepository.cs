using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IBusinessRepository
    {
      

        Task<Business?> GetByIdAsync(Guid id);
        Task<Business?> GetByUserIdAsync(Guid userId); // نحتاجها عشان نعرف البيزنس بتاع اليوزر ده
        Task AddAsync(Business business); // إضافة جديدة
        Task AddVerificationAsync(BusinessVerification verification); // إضافة طلب التوثيق
        Task<BusinessVerification?> GetVerificationByBusinessIdAsync(Guid businessId); // عشان الأدمن يجيب الطلب
        Task UpdateAsync(Business business);
        Task UpdateVerificationAsync(BusinessVerification verification);

        Task<List<Business>> GetPendingVerificationsAsync();


        // 2. جلب أحدث طلب توثيق لشركة معينة (عشان نعدل حالته)
        Task<BusinessVerification?> GetLatestVerificationByBusinessIdAsync(Guid businessId);



    }
}
