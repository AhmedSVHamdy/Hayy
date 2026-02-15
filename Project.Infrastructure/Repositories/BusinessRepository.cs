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

        // =========================================================
        //  إدارة البيزنس
        // =========================================================

        // 👇 غيرنا الاسم من AddAsync لـ AddBusinessAsync
        public async Task AddBusinessAsync(Business business)
        {
            await _context.Businesses.AddAsync(business);
            await _context.SaveChangesAsync();
        }

        // 👇 غيرنا الاسم لـ GetBusinessByIdAsync
        public async Task<Business?> GetBusinessByIdAsync(Guid id)
        {
            return await _context.Businesses
                .Include(b => b.User)
                .Include(b => b.Verifications)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        // 👇👇👇 هذا هو الحل الأساسي للمشكلة 👇👇👇
        // غيرنا الاسم من GetByUserIdAsync لـ GetBusinessByUserIdAsync
        public async Task<Business?> GetBusinessByUserIdAsync(Guid userId)
        {
            return await _context.Businesses
                .Include(b => b.Verifications)
                .FirstOrDefaultAsync(b => b.UserId == userId);
        }

        // 👇 غيرنا الاسم لـ UpdateBusinessAsync
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
    }
}