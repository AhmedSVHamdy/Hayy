using Project.Core.DTO;

namespace Project.Core.ServiceContracts
{
    public interface IInterestService
    {
        Task<List<CategoryWithTagsDTO>> GetAllInterestsAsync();
        Task<bool> SaveUserInterestsAsync(Guid userId, UserInterestRequestDTO request);
    }
}
