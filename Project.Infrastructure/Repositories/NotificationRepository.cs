using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly HayyContext _context;

        public NotificationRepository(HayyContext context)
        {
            _context = context;
        }
        // 1. إضافة
        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        // 2. تعديل
        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        // 3. جلب بالـ ID
        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        // 4. جلب بالـ ID والـ UserId (حماية)
        public async Task<Notification?> GetByIdAndUserIdAsync(Guid id, Guid userId)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        }

        // 5. جلب غير المقروء حسب الجروب (مهمة لمنطق التجميع في السيرفس)
        public async Task<Notification?> GetUnreadByGroupKeyAsync(Guid userId, string groupKey)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.UserId == userId
                                       && n.GroupKey == groupKey
                                       && !n.IsRead);
        }

        // 6. جلب القائمة (Paged)
        public async Task<List<Notification>> GetByUserIdPagedAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt) // الأحدث فوق
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking() // أسرع للعرض
                .ToListAsync();
        }

        // 7. العدد الكلي
        public async Task<int> GetTotalCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId);
        }

        // 8. العدد غير المقروء
        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        // 9. تعليم الكل كمقروء (الأسرع)
        public async Task MarkAllAsReadAsync(Guid userId)
        {
            // جملة SQL مباشرة لتحديث كل الصفوف مرة واحدة
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Notifications SET IsRead = 1 WHERE UserId = {0} AND IsRead = 0",
                userId
            );

            // ملحوظة: لو شغال PostgreSQL خليها: IsRead = true
        }
        public async Task <int> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            // تحديث مباشر وسريع جداً بدون تحميل الكائن للميموري
          return  await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Notifications SET IsRead = 1 WHERE Id = {0} AND UserId = {1}",
                notificationId, userId
            );

            // ملحوظة: لو بتستخدم PostgreSQL خليها: IsRead = true
        }
    }
}
