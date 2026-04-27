using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateOfferDTO;

namespace Project.Core.ServiceContracts
{
    public interface IOfferService
    {
        Task<OfferResponseDto> CreateOfferAsync(CreateOfferDto dto);
        Task<IEnumerable<OfferResponseDto>> GetOffersByPlaceIdAsync(Guid placeId);
        Task<OfferResponseDto> UpdateOfferAsync(Guid id, UpdateOfferDto dto);
        Task<bool> DeleteOfferAsync(Guid id);
        Task ExpireFinishedOffersAsync();
        Task<IEnumerable<OfferResponseDto>> GetActiveOffersAsync();
    }
}
