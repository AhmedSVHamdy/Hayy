using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IReviewRepository
    {
        // 1. دالة إضافة ريفيو جديد
        Task<Review> AddReviewAsync(Review review);

        // 2. (القديمة) بتجيب كله - ممكن تخليها لو محتاجها في حتة تانية، بس الأساسي هيبقى الـ Paged
        Task<IEnumerable<Review>> GetReviewsByPlaceIdAsync(Guid placeId);

        // ✅ 3. (الجديدة) دالة الجلب بالصفحات (Pagination)
        // دي اللي السيرفس كانت بتعيط عليها وبتقول مش لاقياها
        Task<List<Review>> GetReviewsPagedAsync(Guid placeId, int pageNumber, int pageSize);

        // ✅ 4. (الجديدة) دالة عد الريفيوهات
        // عشان نعرف نحسب عدد الصفحات الكلي (TotalPages)
        Task<int> GetCountByPlaceIdAsync(Guid placeId);

        // 5. دالة التحقق من التكرار
        Task<bool> HasUserReviewedPlaceAsync(Guid userId, Guid placeId);
    }
}
