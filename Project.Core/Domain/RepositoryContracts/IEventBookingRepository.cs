using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateEventBooking;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IEventBookingRepository
    {
        Task<EventBooking?> GetByIdAsync(Guid id);
        Task<IEnumerable<EventBooking>> GetUserBookingsAsync(Guid userId);
        Task AddAsync(EventBooking booking);
        Task UpdateAsync(EventBooking booking);
        // ميثود عشان تجيب آخر رقم في طابور الانتظار للإيفنت ده
        Task<int> GetMaxWaitlistPositionAsync(Guid eventId);
        Task<EventBooking?> GetNextInWaitlistAsync(Guid eventId);
        Task<EventBooking?> GetBookingByUserAndEventAsync(Guid userId, Guid eventId);
        Task<IEnumerable<EventBooking>> GetWaitlistedBookingsWithUsersAsync(Guid eventId);
        // الدالة دي هترجعلنا عدد التذاكر المؤكدة لحدث معين
        Task<int> GetConfirmedTicketsCountAsync(Guid eventId);
        Task<int> GetValidTicketsCountAsync(Guid eventId);
        Task<int> GetWaitlistTicketsCountAsync(Guid eventId);
    }
}
