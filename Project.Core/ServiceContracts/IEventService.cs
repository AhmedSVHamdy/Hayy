using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateEventDTO;

namespace Project.Core.ServiceContracts
{
    public interface IEventService
    {
        Task<EventResponseDto> CreateEventAsync(EventCreateDto createDto);
        Task<EventResponseDto?> GetEventByIdAsync(Guid id);
        Task<IEnumerable<EventResponseDto>> GetActiveEventsAsync(); // أو اسم الـ DTO بتاعك
    }
}
