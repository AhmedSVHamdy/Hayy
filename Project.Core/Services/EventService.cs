using AutoMapper;
using FluentValidation;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateEventDTO;

namespace Project.Core.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<EventCreateDto> _validator;
        private readonly INotifier _notifier; // SignalR Hub Wrapper
        private readonly IPlaceRepository _placeRepository; 

        public EventService(
            IEventRepository eventRepository,
            IMapper mapper,
            IValidator<EventCreateDto> validator,
            INotifier notifier,
            IPlaceRepository placeRepository)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _validator = validator;
            _notifier = notifier;
            _placeRepository = placeRepository;
        }

        public async Task<EventResponseDto> CreateEventAsync(EventCreateDto createDto)
        {
            // 1. Validation
            var validationResult = await _validator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                // بنجمع الأخطاء ونرمي Exception أو نرجع أوبجيكت فيه الأخطاء (حسب ستايل مشروعك)
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException($"بيانات غير صالحة: {errors}");
            }

            // 2. Mapping
            var newEvent = _mapper.Map<Event>(createDto);
            newEvent.Status = Enums.EventStatus.Active;

            // 3. Save to DB
            await _eventRepository.AddAsync(newEvent);

            // 4. SignalR Notification (إعلام المتابعين للمكان)
            string groupName = $"Management_{newEvent.PlaceId}"; // أو جروب المتابعين Followers_PlaceId
            await _notifier.SendNotificationToGroup(
                groupName,
                $"إيفنت جديد متاح الآن: {newEvent.Title}! 🎉 سارع بالحجز."
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
    }
}
