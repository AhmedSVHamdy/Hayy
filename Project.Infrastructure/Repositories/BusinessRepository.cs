using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.Enums;
using Project.Infrastructure.ApplicationDbContext;

namespace Project.Infrastructure.Repositories
{
    public class BusinessRepository : IBusinessRepository
    {
        private readonly HayyContext _context;

        public BusinessRepository(HayyContext context)
        {
            _context = context;
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task AddBusinessAnalyticAsync(BusinessAnalytic analytic)
        {
            await _context.BusinessAnalytics.AddAsync(analytic);
        }

        // =========================================================
        //  إدارة البيزنس
        // =========================================================

        public async Task AddBusinessAsync(Business business)
        {
            await _context.Businesses.AddAsync(business);
            await _context.SaveChangesAsync();
        }

        public async Task<Business?> GetBusinessByIdAsync(Guid id)
        {
            return await _context.Businesses
                .Include(b => b.User)
                .Include(b => b.BusinessAnalytics)
                .Include(b => b.Verifications)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Business?> GetBusinessByUserIdAsync(Guid userId)
        {
            return await _context.Businesses
                .Include(b => b.Verifications)
                .Include(b => b.BusinessAnalytics)

                // ✅ جلب الاشتراكات ومعها الخطة المرتبطة بها لمعرفة السعر وصلاحية الاشتراك
                .Include(b => b.Subscriptions)
                    .ThenInclude(s => s.Plan)

                // 🚀🚀 التعديل الأهم: 
                // تم حذف الـ Includes الخاصة بالـ Places والـ Reviews والـ Follows من هنا!
                // الدالة دي بقت سريعة جداً وهتعتمد على قراءة البيانات الجاهزة من جدول الـ BusinessAnalytics
                .FirstOrDefaultAsync(b => b.UserId == userId);
        }

        public async Task UpdateBusinessAsync(Business business)
        {
            _context.Businesses.Update(business);
            await _context.SaveChangesAsync();
        }

        // =========================================================
        //  إدارة التوثيق
        // =========================================================

        public async Task AddVerificationAsync(BusinessVerification verification)
        {
            await _context.BusinessVerifications.AddAsync(verification);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVerificationAsync(BusinessVerification verification)
        {
            _context.BusinessVerifications.Update(verification);
            await _context.SaveChangesAsync();
        }

        public async Task<BusinessVerification?> GetLatestVerificationByBusinessIdAsync(Guid businessId)
        {
            return await _context.BusinessVerifications
                .Where(v => v.BusinessId == businessId)
                .OrderByDescending(v => v.SubmittedAt)
                .FirstOrDefaultAsync();
        }

        // =========================================================
        //  دوال الأدمن
        // =========================================================

        public async Task<List<Business>> GetPendingVerificationsAsync()
        {
            return await _context.Businesses
                .Include(b => b.Verifications)
                .Where(b => b.VerificationStatus == VerificationStatus.Pending)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        // =========================================================
        //  دوال المزامنة والسكربتات 
        // =========================================================
        public async Task<List<Business>> GetAllBusinessesWithDetailsForSyncAsync()
        {
            return await _context.Businesses
                .Include(b => b.BusinessAnalytics)
                .Include(b => b.Subscriptions).ThenInclude(s => s.Plan)
                .Include(b => b.Places).ThenInclude(p => p.Reviews)
                .Include(b => b.Places).ThenInclude(p => p.PlaceFollows)
                .ToListAsync();
        }
    }
}