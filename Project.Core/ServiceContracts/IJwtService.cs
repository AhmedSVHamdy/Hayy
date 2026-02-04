using Project.Core.Domain.Entities;
using Project.Core.DTO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project.Core.ServiceContracts
{
    public interface IJwtService
    {
        Task<AuthenticationResponse> CreateJwtTokenAsync(User user ,string clientType);

        Task<ClaimsPrincipal?> GetPrincipalFromJwtToken(string? token);

    }

}
