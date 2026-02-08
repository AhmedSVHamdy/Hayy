using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;

namespace Project.Core.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepo;

        public AdminService(IAdminRepository adminRepo)
        {
            _adminRepo = adminRepo;
        }

        public async Task LogAdminActionAsync(Guid adminId, AdminActionType actionType, TargetType targetType, string targetId, string? notes = null)
        {
            var action = new AdminAction
            {
                Id = Guid.NewGuid(),
                AdminId = adminId,
                ActionType = actionType,
                TargetType = targetType,
                TargetId = targetId,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            await _adminRepo.LogActionAsync(action);
        }

        public async Task<List<AdminActionResponse>> GetAuditLogAsync()
        {
            var logs = await _adminRepo.GetAdminActionsAsync();

            // تحويل Entity لـ DTO
            return logs.Select(x => new AdminActionResponse
            {
                Id = x.Id,
                AdminName = x.Admin?.FullName ?? "Unknown",
                ActionType = x.ActionType.ToString(),
                TargetType = x.TargetType.ToString(),
                TargetId = x.TargetId,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            }).ToList();
        }

        public async Task<DashboardStatsDTO> GetDashboardStatisticsAsync()
        {
            return await _adminRepo.GetDashboardStatsAsync();
        }
    }
}