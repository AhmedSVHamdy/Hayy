using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.Enums;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateEventBooking;

namespace Project.Infrastructure.Repositories
{
    public class EventBookingRepository : IEventBookingRepository
    {
        private readonly HayyContext _context;

        public EventBookingRepository(HayyContext context)
        {
            _context = context;
            
        }

        public async Task<EventBooking?> GetByIdAsync(Guid id)
        {
            return await _context.EventBookings
                .Include(b => b.Event)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<EventBooking>> GetUserBookingsAsync(Guid userId)
        {
            return await _context.EventBookings
                .Include(b => b.Event)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.PaymentDate) // ترتيب من الأحدث
                .ToListAsync();
        }

        public async Task AddAsync(EventBooking booking)
        {
            await _context.EventBookings.AddAsync(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(EventBooking booking)
        {
            _context.EventBookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetMaxWaitlistPositionAsync(Guid eventId)
        {
            var maxPosition = await _context.EventBookings
                .Where(b => b.EventId == eventId && b.WaitlistPosition != null)
                .MaxAsync(b => (int?)b.WaitlistPosition);

            return maxPosition ?? 0; // لو مفيش حد، رجع صفر
        }
        public async Task<EventBooking?> GetNextInWaitlistAsync(Guid eventId)
        {
            return await _context.EventBookings
                .Include(b => b.User)  // عشان نجيب إيميل اليوزر
                .Include(b => b.Event) // عشان نجيب اسم الحدث
                .Where(b => b.EventId == eventId && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync();
        }
        public async Task<EventBooking?> GetBookingByUserAndEventAsync(Guid userId, Guid eventId)
        {
            return await _context.EventBookings
                .Include(b => b.Event) // عشان لو الـ DTO محتاج بيانات الإيفنت (اسمه، صورته)
                .FirstOrDefaultAsync(b => b.UserId == userId && b.EventId == eventId);
        }
        public async Task<IEnumerable<EventBooking>> GetWaitlistedBookingsWithUsersAsync(Guid eventId)
        {
            return await _context.EventBookings
                .Include(b => b.User) // 💡 دي أهم حاجة عشان نجيب الإيميلات
                .Where(b => b.EventId == eventId && b.Status == BookingStatus.Waitlisted)
                .ToListAsync();
        }
        public async Task<int> GetConfirmedTicketsCountAsync(Guid eventId)
        {
            return await _context.EventBookings
                .Where(b => b.EventId == eventId && b.Status == BookingStatus.Confirmed)
                .SumAsync(b => b.TicketQuantity);
        }
        public async Task<int> GetValidTicketsCountAsync(Guid eventId)
        {
            return await _context.EventBookings
         .Where(b => b.EventId == eventId &&
                    (b.Status == BookingStatus.Confirmed ||
                     b.Status == BookingStatus.Pending ||
                     b.Status == BookingStatus.Used ||      // 🔥 ضفنا الـ Used
                     b.Status == BookingStatus.Completed))  // 🔥 وضفنا الـ Completed بالمرة لضمان الأمان
         .SumAsync(b => b.TicketQuantity);
        }
        public async Task<int> GetWaitlistTicketsCountAsync(Guid eventId)
        {
            return await _context.EventBookings
                .Where(b => b.EventId == eventId && b.Status == BookingStatus.Waitlisted)
                .SumAsync(b => b.TicketQuantity);
        }
    }
}
