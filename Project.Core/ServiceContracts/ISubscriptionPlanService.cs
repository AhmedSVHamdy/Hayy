using Project.Core.DTO.Plans;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface ISubscriptionPlanService
    {
        Task<IEnumerable<SubscriptionPlanResponseDto>> GetAllAsync();
        Task<SubscriptionPlanResponseDto?> GetByIdAsync(Guid id);
        Task<SubscriptionPlanResponseDto> AddPlanAsync(AddSubscriptionPlanRequestDto dto);
        Task<SubscriptionPlanResponseDto?> UpdatePlanAsync(Guid id, UpdateSubscriptionPlanRequestDto dto);

        Task<bool> DeletePlanAsync(Guid id); // Soft Delete
    }
}
