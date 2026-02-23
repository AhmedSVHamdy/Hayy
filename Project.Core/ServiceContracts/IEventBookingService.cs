using Project.Core.DTO;
using Project.Core.DTO.Paymob;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateEventBooking;

namespace Project.Core.ServiceContracts
{
    public interface IEventBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(Guid userId, CreateBookingDto dto);
        Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(Guid userId);
        Task<BookingResponseDto> ConfirmPaymentAsync(Guid userId, ConfirmPaymentDto dto);
        Task<VerifyTicketResultDto> VerifyTicketAsync(Guid businessUserId, Guid bookingId);
        Task CancelUnpaidBookingAsync(Guid bookingId);
        Task<BookingResponseDto?> GetUserBookingForEventAsync(Guid userId, Guid eventId);

    }
}
