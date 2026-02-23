using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.Enums;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateEventDTO;

namespace Project.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly HayyContext _context;

        public EventRepository(HayyContext context)
        {
            _context = context;
        }

        public async Task<Event?> GetByIdAsync(Guid id)
        {
            return await _context.Events
                    .Include(e => e.EventBookings) // 👈 لازم السطر ده يكون موجود هنا كمان
                    .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Event>> GetByPlaceIdAsync(Guid placeId)
        {
            return await _context.Events.Where(e => e.PlaceId == placeId).ToListAsync();
        }

        public async Task AddAsync(Event @event)
        {
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Event>> GetActiveEventsAsync()
        {
            // بنجيب الداتا بس من الداتابيز من غير أي تحويل (Select)
            var activeEvents = await _context.Events
                .Include(e => e.EventBookings)
                .Where(e => e.Datetime > DateTime.UtcNow && e.Status == EventStatus.Active)
                .ToListAsync();

            return activeEvents;
        }
    }

}
