using Project.Core.Domain.Entities;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IUserInterestRepository
    {
        Task AddRangeAsync(IEnumerable<UserInterestProfile> interests);
        Task<IEnumerable<UserInterestProfile>> GetUserInterestsByUserIdAsync(Guid userId);
        Task SaveChangesAsync(); // أو ممكن تدمجها مع الدالة اللي فوق

    }
}
