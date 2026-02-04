using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RopositoryContracts
{
    public interface INotificationRepository
    {
        // 1. الإضافة (Create)
        Task AddAsync(Notification notification);

        // 2. التعديل (Update) - دي بديلة لـ MarkAsRead
        Task UpdateAsync(Notification notification);

        // 3. جلب عنصر واحد بالـ ID
        Task<Notification?> GetByIdAsync(Guid id);

        // 4. جلب كل إشعارات اليوزر (History)
        Task<List<Notification>> GetByUserIdAsync(Guid userId);

        // 5. جلب غير المقروء (للتجميع Grouping)
        Task<Notification?> GetUnreadByGroupKeyAsync(Guid userId, string groupKey);

        // 6. جلب غير المقروء (عشان زرار Mark All)
        Task<List<Notification>> GetUnreadByUserIdAsync(Guid userId);

        // 7. العداد (Counter)
        Task<int> CountUnreadAsync(Guid userId);

        // 8. تحديث مجموعة (عشان Mark All تبقى سريعة)
        Task UpdateRangeAsync(IEnumerable<Notification> notifications);
    }
}
