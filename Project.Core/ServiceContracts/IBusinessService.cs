using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface IBusinessService
    {

        // 1. دالة تقديم بيانات البيزنس (Onboarding)
        Task SubmitBusinessDetailsAsync(Guid userId, BusinessOnboardingDTO model);

        // 2. دالة جلب قائمة الطلبات المعلقة للأدمن
        Task<List<BusinessVerificationSummaryDTO>> GetPendingVerificationsAsync();

        // 3. دالة مراجعة الأدمن (Approve / Reject)
        Task ReviewBusinessAsync(Guid businessId, ReviewBusinessDTO reviewDto, Guid adminId);

    }
}
