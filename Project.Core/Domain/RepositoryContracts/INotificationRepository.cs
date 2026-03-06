using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
namespace Project.Core.Domain.RepositoryContracts
{
    public interface INotificationRepository
    {
        // 1. إضافة
        Task AddAsync(Notification notification);

        // 2. تعديل
        Task UpdateAsync(Notification notification);

        // 3. جلب بالـ ID (بنحتاجه أحياناً)
        Task<Notification?> GetByIdAsync(Guid id);

        // 4. جلب بالـ ID والـ UserId (للحماية قبل القراءة)
        Task<Notification?> GetByIdAndUserIdAsync(Guid id, Guid userId);

        // 5. جلب غير المقروء حسب الجروب (مهم عشان تجميع الإشعارات في السيرفس)
        Task<Notification?> GetUnreadByGroupKeyAsync(Guid userId, string groupKey);

        // 6. جلب القائمة (Pagination)
        Task<List<Notification>> GetByUserIdPagedAsync(Guid userId, int pageNumber, int pageSize);

        // 7. العدد الكلي (عشان الـ Pagination)
        Task<int> GetTotalCountAsync(Guid userId);

        // 8. العدد غير المقروء (عشان العداد الأحمر)
        Task<int> GetUnreadCountAsync(Guid userId);

        // 9. تعليم الكل كمقروء (جملة SQL مباشرة)
        Task MarkAllAsReadAsync(Guid userId);

        Task<int> MarkAsReadAsync(Guid notificationId, Guid userId);
    }
}
