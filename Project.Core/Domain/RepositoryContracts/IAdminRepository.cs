using Project.Core.Domain.Entities;
using Project.Core.DTO;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IAdminRepository
    {
        // 1. تسجيل حركة جديدة (زي الموافقة على بيزنس، حظر يوزر، إلخ)
        Task LogActionAsync(AdminAction action);

        // 2. جلب سجل الحركات بالكامل لعرضه في الـ Audit Log
        Task<List<AdminAction>> GetAdminActionsAsync();

        // 3. جلب إحصائيات الصفحة الرئيسية للداشبورد (عدد المستخدمين، البيزنس، إلخ)
        Task<DashboardStatsDTO> GetDashboardStatsAsync();

    }

}
