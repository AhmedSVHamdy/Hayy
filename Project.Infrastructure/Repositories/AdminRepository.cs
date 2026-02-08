using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Infrastructure.ApplicationDbContext;


namespace Project.Infrastructure.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly HayyContext _hayy;

        public AdminRepository(HayyContext context)
        {
            _hayy = context;
        }

        // تسجيل حركة جديدة
        public async Task LogActionAsync(AdminAction action)
        {
            await _hayy.AdminActions.AddAsync(action);
            await _hayy.SaveChangesAsync();
        }

        // جلب السجل كامل (الأحدث أولاً)
        public async Task<List<AdminAction>> GetAdminActionsAsync()
        {
            return await _hayy.AdminActions
                .Include(a => a.Admin) // عشان نعرض اسم الأدمن
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // إحصائيات الداشبورد (Dashboard Stats)
        public async Task<DashboardStatsDTO> GetDashboardStatsAsync()
        {
            return new DashboardStatsDTO
            {
                TotalUsers = await _hayy.Users.CountAsync(),
                TotalBusinesses = await _hayy.Businesses.CountAsync(),
                PendingVerifications = await _hayy.Businesses
                    .CountAsync(b => b.VerificationStatus == VerificationStatus.Pending),
                VerifiedBusinesses = await _hayy.Businesses
                    .CountAsync(b => b.VerificationStatus == VerificationStatus.Verified)
            };
        }
    }
}
