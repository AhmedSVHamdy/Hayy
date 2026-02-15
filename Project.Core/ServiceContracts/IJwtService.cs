using Project.Core.Domain.Entities;
using Project.Core.DTO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project.Core.ServiceContracts
{
    public interface IJwtService
    {
        // لاحظ إضافة clientType كـ optional parameter
        Task<AuthenticationResponse> CreateJwtTokenAsync(User user, string clientType = "Web");

        // دالة لاستخراج البيانات من التوكن المنتهي (عشان الـ Refresh Token Flow)
        Task<ClaimsPrincipal?> GetPrincipalFromJwtToken(string? token);
    }

}
