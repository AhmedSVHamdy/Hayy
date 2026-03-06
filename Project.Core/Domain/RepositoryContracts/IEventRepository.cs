using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateEventDTO;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(Guid id);
        Task<IEnumerable<Event>> GetByPlaceIdAsync(Guid placeId);
        Task AddAsync(Event @event);
        Task<IEnumerable<Event>> GetActiveEventsAsync();
        // بدل Event خليها ترجع الـ DTO
    }
}
