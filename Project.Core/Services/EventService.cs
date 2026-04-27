using AutoMapper;
using FluentValidation;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Project.Core.DTO.CreateEventDTO;
using Hangfire; // 👈 1. ضيفنا النيم سبيس ده

namespace Project.Core.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<EventCreateDto> _validator;
        private readonly INotifier _notifier;
        private readonly IPlaceRepository _placeRepository; 
        private readonly IBackgroundJobClient _backgroundJobClient; // 👈 2. ضيفنا العسكري بتاع Hangfire

        public EventService(
            IEventRepository eventRepository,
            IMapper mapper,
            IValidator<EventCreateDto> validator,
            INotifier notifier,
            IPlaceRepository placeRepository,
            IBackgroundJobClient backgroundJobClient) // 👈 3. حقناه هنا
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _validator = validator;
            _notifier = notifier;
            _placeRepository = placeRepository;
            _backgroundJobClient = backgroundJobClient; // 👈 4. وربطناه هنا
        }

        public async Task<EventResponseDto> CreateEventAsync(EventCreateDto createDto)
        {
            // 1. Validation
            var validationResult = await _validator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException($"بيانات غير صالحة: {errors}");
            }

            // 2. Mapping
            var newEvent = _mapper.Map<Event>(createDto);
            newEvent.Status = Enums.EventStatus.Active;

            // 3. Save to DB
            await _eventRepository.AddAsync(newEvent);

            // 4. 🔥 Hangfire Background Job 🔥 (بدل القديم)
            string title = "إيفنت جديد متاح الآن! 🎉";
            string msg = $"لا تفوت فرصة الحضور في إيفنت: {newEvent.Title}. سارع بالحجز!";

            _backgroundJobClient.Enqueue<INotificationService>(service => 
                service.NotifyFollowersBackgroundJobAsync(
                    newEvent.PlaceId, 
                    title, 
                    msg, 
                    newEvent.Id.ToString(), 
                    ReferenceType.Event.ToString(), // 👈 حددنا إنه إيفنت عشان الفرونت وتوجيه الصفحات
                    NotificationType.EventAlert.ToString() // 👈 حددنا نوع الإشعار
                )
            );

            // 5. Return Response
            return _mapper.Map<EventResponseDto>(newEvent);
        }
        
        public async Task<EventResponseDto?> GetEventByIdAsync(Guid id)
        {
            var @event = await _eventRepository.GetByIdAsync(id);
            if (@event == null) return null;
            return _mapper.Map<EventResponseDto>(@event);
        }
        public async Task<IEnumerable<EventResponseDto>> GetActiveEventsAsync()
        {
            var events = await _eventRepository.GetActiveEventsAsync();

            return _mapper.Map<IEnumerable<EventResponseDto>>(events);
        }

        public async Task<EventResponseDto> UpdateEventAsync(Guid eventId, UpdateEventDto dto, Guid userId)
        {
            var existingEvent = await _eventRepository.GetByIdAsync(eventId);
            if (existingEvent == null)
                throw new KeyNotFoundException("عذراً، هذا الحدث (Event) غير موجود.");

            // 🛑 حماية: التأكد إن اليوزر هو صاحب المكان
            var place = await _placeRepository.GetByIdAsync(existingEvent.PlaceId);
            
            // تحديث البيانات 
            _mapper.Map(dto, existingEvent);

                        
                await _eventRepository.UpdateAsync(existingEvent);
            
            

            return _mapper.Map<EventResponseDto>(existingEvent);
        }

        public async Task DeleteEventAsync(Guid eventId, Guid userId)
        {
            var existingEvent = await _eventRepository.GetByIdAsync(eventId);
            if (existingEvent == null)
                throw new KeyNotFoundException("عذراً، هذا الحدث غير موجود مسبقاً.");

            // 🛑 حماية: التأكد إن اليوزر هو صاحب المكان
            var place = await _placeRepository.GetByIdAsync(existingEvent.PlaceId);
            await _eventRepository.DeleteAsync(existingEvent);
        }
    }
}
