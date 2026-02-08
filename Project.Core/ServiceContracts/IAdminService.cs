using Project.Core.DTO;
using Project.Core.Enums;

namespace Project.Core.ServiceContracts
{
    public interface IAdminService
    {
        // دالة بننادي عليها من أي مكان في الكود عشان نسجل حركة
        Task LogAdminActionAsync(Guid adminId, AdminActionType actionType, TargetType targetType, string targetId, string? notes = null);

        Task<List<AdminActionResponse>> GetAuditLogAsync();

        Task<DashboardStatsDTO> GetDashboardStatisticsAsync();
    }
}
