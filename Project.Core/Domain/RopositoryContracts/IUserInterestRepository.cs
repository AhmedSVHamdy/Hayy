using Project.Core.Domain.Entities;

namespace Project.Core.Domain.RopositoryContracts
{
    public interface IUserInterestRepository
    {
        Task AddRangeAsync(IEnumerable<UserInterestProfile> interests);
        Task SaveChangesAsync(); // أو ممكن تدمجها مع الدالة اللي فوق
    }
}
