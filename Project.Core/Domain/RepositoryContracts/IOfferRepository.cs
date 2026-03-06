using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IOfferRepository
    {
        Task<Offer?> GetByIdAsync(Guid id);
        Task<IEnumerable<Offer>> GetOffersByPlaceIdAsync(Guid placeId);
        Task<IEnumerable<Offer>> GetActiveOffersAsync(); // للعروض الشغالة بس
        Task<Offer> AddAsync(Offer offer);
        Task UpdateAsync(Offer offer);
        Task DeleteAsync(Offer offer);
        Task SaveChangesAsync();
        Task<IEnumerable<Offer>> GetExpiredActiveOffersAsync(DateTime currentDate);
    }
}
