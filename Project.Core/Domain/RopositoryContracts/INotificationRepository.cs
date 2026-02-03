using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RopositoryContracts
{
    public interface INotificationRepository
    {
        // إضافة إشعار للداتابيز
        Task<Notification> AddAsync(Notification notification);

        // يجيب كل إشعارات يوزر معين (الأحدث فالأقدم)
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId);

        // يجيب إشعار واحد بال ID
        Task<Notification?> GetByIdAsync(Guid id);

        // يعلم على الإشعار إنه اتقرأ
        Task MarkAsReadAsync(Notification notification);

        Task<Notification?> GetUnreadByGroupKeyAsync(Guid userId, string groupKey);

        Task UpdateAsync(Notification notification);
    }
}
