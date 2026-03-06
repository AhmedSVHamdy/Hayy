using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.Enums;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class OfferRepository : IOfferRepository
    {
        private readonly HayyContext _context;

        public OfferRepository(HayyContext context)
        {
            _context = context;
        }

        public async Task<Offer?> GetByIdAsync(Guid id) =>
            await _context.Offers.FirstOrDefaultAsync(o => o.Id == id);

        public async Task<IEnumerable<Offer>> GetOffersByPlaceIdAsync(Guid placeId) =>
            await _context.Offers.Where(o => o.PlaceId == placeId).ToListAsync();

        public async Task<IEnumerable<Offer>> GetActiveOffersAsync() =>
            await _context.Offers
                .Where(o => o.Status == OfferStatus.Active && o.EndDate >= DateTime.UtcNow)
                .ToListAsync();

        public async Task<Offer> AddAsync(Offer offer)
        {
            await _context.Offers.AddAsync(offer);
            return offer;
        }

        public async Task UpdateAsync(Offer offer) => _context.Offers.Update(offer);

        public async Task DeleteAsync(Offer offer) => _context.Offers.Remove(offer);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<IEnumerable<Offer>> GetExpiredActiveOffersAsync(DateTime currentDate)
        {
            // هنجيب كل العروض اللي حالتها لسه "Active" بس تاريخ انتهائها أقدم من دلوقتي
            return await _context.Offers
                .Where(o => o.Status == OfferStatus.Active && o.EndDate < currentDate)
                .ToListAsync();
        }
    }
}
