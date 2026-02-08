using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.Enums;
using Project.Infrastructure.ApplicationDbContext; // تأكد أن هذا هو الـ Namespace الصحيح للـ Context

namespace Project.Infrastructure.Repositories
{
    public class BusinessRepository : IBusinessRepository
    {
        private readonly HayyContext _context;

        public BusinessRepository(HayyContext context)
        {
            _context = context;
        }

        // =========================================================
        //  إدارة البيزنس (CRUD)
        // =========================================================

        public async Task AddAsync(Business business)
        {
            await _context.Businesses.AddAsync(business);
            await _context.SaveChangesAsync();
        }

        public async Task<Business?> GetByIdAsync(Guid id)
        {
            return await _context.Businesses
                .Include(b => b.User) // نحتاج بيانات صاحب البيزنس أحياناً
                .Include(b => b.Verifications) // وتاريخ التوثيق
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Business?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Businesses
                .Include(b => b.Verifications)
                .FirstOrDefaultAsync(b => b.UserId == userId);
        }

        public async Task UpdateAsync(Business business)
        {
            _context.Businesses.Update(business);
            await _context.SaveChangesAsync();
        }

        // =========================================================
        //  إدارة التوثيق (Verification Management)
        // =========================================================

        public async Task AddVerificationAsync(BusinessVerification verification)
        {
            await _context.BusinessVerifications.AddAsync(verification);
            await _context.SaveChangesAsync();
        }

        // 🟢 الدالة دي مهمة جداً للأدمن وللبيزنس عشان يعرف حالة آخر طلب
        public async Task<BusinessVerification?> GetLatestVerificationByBusinessIdAsync(Guid businessId)
        {
            return await _context.BusinessVerifications
                .Where(v => v.BusinessId == businessId)
                .OrderByDescending(v => v.SubmittedAt) // بنجيب أحدث واحد
                .FirstOrDefaultAsync();
        }

        // 🟢 تنفيذ التحديث اللي كان ناقص عندك
        public async Task UpdateVerificationAsync(BusinessVerification verification)
        {
            _context.BusinessVerifications.Update(verification);
            await _context.SaveChangesAsync();
        }

        // =========================================================
        //  دوال الأدمن (Admin Dashboard)
        // =========================================================

        public async Task<List<Business>> GetPendingVerificationsAsync()
        {
            return await _context.Businesses
                .Include(b => b.Verifications) // عشان نعرض تاريخ التقديم
                .Where(b => b.VerificationStatus == VerificationStatus.Pending)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<BusinessVerification?> GetVerificationByBusinessIdAsync(Guid businessId)
        {
            return await _context.BusinessVerifications
                .Include(v => v.Business) // بنجيب بيانات البيزنس المرتبطة بالطلب
                .Where(v => v.BusinessId == businessId)
                .OrderByDescending(v => v.SubmittedAt) // ⚠️ مهم جداً: بنرتب عشان نجيب أحدث طلب
                .FirstOrDefaultAsync();
        }
    }
}