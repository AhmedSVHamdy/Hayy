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
using Hangfire;

namespace Project.Core.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<EventCreateDto> _validator;
        private readonly INotifier _notifier;
        private readonly IPlaceRepository _placeRepository;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IImageService _imageService; // 👈 1. ضفنا سيرفس الصور

        public EventService(
            IEventRepository eventRepository,
            IMapper mapper,
            IValidator<EventCreateDto> validator,
            INotifier notifier,
            IPlaceRepository placeRepository,
            IBackgroundJobClient backgroundJobClient,
            IImageService imageService) // 👈 2. عملنالها حقن هنا
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _validator = validator;
            _notifier = notifier;
            _placeRepository = placeRepository;
            _backgroundJobClient = backgroundJobClient;
            _imageService = imageService; // 👈 3. ربطناها
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

            // 👈 4. رفع الصورة لو موجودة وحفظ اللينك في الداتابيز
            if (createDto.ImageFile != null && createDto.ImageFile.Length > 0)
            {
                newEvent.GalleryImages = await _imageService.UploadImageAsync(createDto.ImageFile);
            }

            // 3. Save to DB
            await _eventRepository.AddAsync(newEvent);

            // 4. 🔥 Hangfire Background Job 🔥 
            string title = "إيفنت جديد متاح الآن! 🎉";
            string msg = $"لا تفوت فرصة الحضور في إيفنت: {newEvent.Title}. سارع بالحجز!";

            _backgroundJobClient.Enqueue<INotificationService>(service =>
                service.NotifyFollowersBackgroundJobAsync(
                    newEvent.PlaceId,
                    title,
                    msg,
                    newEvent.Id.ToString(),
                    ReferenceType.Event.ToString(),
                    NotificationType.EventAlert.ToString()
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

            // 👈 5. رفع الصورة الجديدة لو بعت واحدة وتحديث اللينك
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                existingEvent.GalleryImages = await _imageService.UploadImageAsync(dto.ImageFile);
            }

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