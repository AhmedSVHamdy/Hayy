using MongoDB.Driver;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RopositoryContracts;
using Project.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class UserLogRepository : IUserLogRepository
    {
        private readonly IMongoCollection<UserLog> _userLogs;

        public UserLogRepository(IMongoDatabase database)
        {
            // ربطنا الكود بجدول (Collection) اسمه UserLogs
            _userLogs = database.GetCollection<UserLog>("UserLogs");
        }

        // ==========================================
        // 1. الإضافة
        // ==========================================
        public async Task AddLogAsync(UserLog log)
        {
            await _userLogs.InsertOneAsync(log);
        }

        // ==========================================
        // 2. القراءة والفلترة (Basic Reads)
        // ==========================================

        public async Task<IEnumerable<UserLog>> GetLogsByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _userLogs.Find(x => x.UserId == userId)
                                  .SortByDescending(x => x.CreatedAt) // الأحدث الأول
                                  .Skip((pageNumber - 1) * pageSize)  // معادلة الباجنيشن
                                  .Limit(pageSize)
                                  .ToListAsync();
        }

        public async Task<IEnumerable<UserLog>> GetLogsByActionTypeAsync(Guid userId, ActionType actionType)
        {
            return await _userLogs.Find(x => x.UserId == userId && x.ActionType == actionType)
                                  .SortByDescending(x => x.CreatedAt)
                                  .ToListAsync();
        }

        public async Task<IEnumerable<UserLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // بنستخدم Builders عشان نعمل Range Query (أكبر من وأصغر من)
            var filter = Builders<UserLog>.Filter.And(
                Builders<UserLog>.Filter.Gte(x => x.CreatedAt, startDate), // Greater Than or Equal
                Builders<UserLog>.Filter.Lte(x => x.CreatedAt, endDate)    // Less Than or Equal
            );

            return await _userLogs.Find(filter)
                                  .SortByDescending(x => x.CreatedAt)
                                  .ToListAsync();
        }

        // ==========================================
        // 3. التحليل والإحصائيات (Aggregation) 🧠
        // ==========================================

        public async Task<IEnumerable<string>> GetTopSearchQueriesAsync(int topCount)
        {
            // 1. فلتر: هات اللي ليهم SearchQuery بس
            var filter = Builders<UserLog>.Filter.Ne(x => x.SearchQuery, null);

            return await _userLogs.Aggregate()
                .Match(filter)
                // 2. جمعهم حسب نص البحث وعددهم
                .Group(l => l.SearchQuery, g => new { Query = g.Key, Count = g.Count() })
                // 3. رتبهم بالتنازلي (الأكثر تكراراً)
                .SortByDescending(x => x.Count)
                // 4. خد العدد المطلوب
                .Limit(topCount)
                // 5. رجع النص بس
                .Project(x => x.Query)
                .ToListAsync();
        }

        public async Task<Dictionary<Guid, int>> GetMostInteractedTargetsAsync(TargetType targetType, int topCount)
        {
            // فلتر حسب النوع وتأكد إن الـ TargetId مش null
            var filter = Builders<UserLog>.Filter.And(
                Builders<UserLog>.Filter.Eq(x => x.TargetType, targetType),
                Builders<UserLog>.Filter.Ne(x => x.TargetId, null)
            );

            var result = await _userLogs.Aggregate()
                .Match(filter)
                // جمع حسب الـ TargetId
                .Group(l => l.TargetId, g => new { TargetId = g.Key, Count = g.Count() })
                .SortByDescending(x => x.Count)
                .Limit(topCount)
                .ToListAsync();

            // تحويل النتيجة لـ Dictionary (عشان سهولة الاستخدام)
            // الـ Key هو الـ ID، والـ Value هو عدد مرات التفاعل
            return result.ToDictionary(k => k.TargetId!.Value, v => v.Count);
        }

        public async Task<int> GetTotalDurationByUserIdAsync(Guid userId)
        {
            var result = await _userLogs.Aggregate()
                .Match(x => x.UserId == userId)
                // اجمع عمود Duration
                .Group(l => l.UserId, g => new { TotalDuration = g.Sum(x => x.Duration) })
                .FirstOrDefaultAsync();

            return result?.TotalDuration ?? 0; // لو ملقاش داتا يرجع صفر
        }
    }
}
