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

        // 1. تنفيذ الإضافة
        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        // 2. تنفيذ التعديل (بيغطي الـ MarkAsRead والـ Update العادي)
        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        // 3. تنفيذ جلب عنصر واحد
        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        // 4. تنفيذ جلب القائمة للمستخدم
        public async Task<List<Notification>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt) // الأحدث فوق
                .Take(50) // ليميت عشان الأداء
                .ToListAsync();
        }

        // 5. تنفيذ البحث عن إشعار للتجميع
        public async Task<Notification?> GetUnreadByGroupKeyAsync(Guid userId, string groupKey)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.UserId == userId
                                       && n.GroupKey == groupKey
                                       && !n.IsRead);
        }

        // 6. تنفيذ جلب غير المقروء فقط
        public async Task<List<Notification>> GetUnreadByUserIdAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
        }

        // 7. تنفيذ العداد
        public async Task<int> CountUnreadAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        // 8. تنفيذ التحديث الجماعي
        public async Task UpdateRangeAsync(IEnumerable<Notification> notifications)
        {
            _context.Notifications.UpdateRange(notifications);
            await _context.SaveChangesAsync();
        }
        public async Task<Notification?> GetByIdAndUserIdAsync(Guid id, Guid userId)
        {
            // لو شغال Entity Framework
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        }

        public async Task<List<Notification>> GetByUserIdPagedAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt) // 👈 مهم جداً: الأحدث الأول
                .Skip((pageNumber - 1) * pageSize)   // يفط الصفحات اللي فاتت
                .Take(pageSize)                      // ياخد عدد معين بس (مثلاً 20)
                .ToListAsync();
        }
        // في الـ Implementation
        public async Task<int> GetCountByUserIdAsync(Guid userId)
        {
            return await _context.Notifications.CountAsync(n => n.UserId == userId);
        }
    }
}
