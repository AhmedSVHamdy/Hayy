using AutoMapper;
using FluentValidation;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq; // عشان الـ First() تشتغل لو مش موجوده عندك
using System.Text;
using static Project.Core.DTO.CreateOfferDTO;
using Hangfire;

namespace Project.Core.Services
{
    public class OfferService : IOfferService
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IPlaceRepository _placeRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateOfferDto> _validator;
        private readonly INotifier _notifier; // SignalR
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IImageService _imageService; // 👈 1. ضفنا سيرفس الصور

        public OfferService(
            IOfferRepository offerRepository,
            IPlaceRepository placeRepository,
            IMapper mapper,
            IValidator<CreateOfferDto> validator,
            INotifier notifier,
            IBackgroundJobClient backgroundJobClient,
            IImageService imageService) // 👈 2. عملنالها حقن هنا
        {
            _offerRepository = offerRepository;
            _placeRepository = placeRepository;
            _mapper = mapper;
            _validator = validator;
            _notifier = notifier;
            _backgroundJobClient = backgroundJobClient;
            _imageService = imageService; // 👈 3. ربطناها
        }

        public async Task<OfferResponseDto> CreateOfferAsync(CreateOfferDto dto)
        {
            // 1. Validation
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ArgumentException(validationResult.Errors.First().ErrorMessage);

            // 2. هل المكان موجود ومفعل؟
            var place = await _placeRepository.GetByIdAsync(dto.PlaceId);
            if (place == null || !place.IsActive)
                throw new InvalidOperationException("عذراً، هذا المكان غير موجود أو غير مفعل.");

            // 3. Mapping & Save
            var offerEntity = _mapper.Map<Offer>(dto);

            // 👈 4. رفع الصورة وحفظ اللينك
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                offerEntity.GalleryImages = await _imageService.UploadImageAsync(dto.ImageFile);
            }

            await _offerRepository.AddAsync(offerEntity);
            await _offerRepository.SaveChangesAsync();

            // 4. 🔥 Hangfire Background Job 🔥
            string title = $"عرض جديد من {place.Name}! 🎉";
            string msg = $"استمتع بخصم {dto.Discount}% على {dto.Title}";

            _backgroundJobClient.Enqueue<INotificationService>(service =>
                service.NotifyFollowersBackgroundJobAsync(
                    dto.PlaceId,
                    title,
                    msg,
                    offerEntity.Id.ToString(),
                    ReferenceType.Offer.ToString(),
                    NotificationType.OfferAlert.ToString()
                )
            );

            return _mapper.Map<OfferResponseDto>(offerEntity);
        }

        public async Task<IEnumerable<OfferResponseDto>> GetActiveOffersAsync()
        {
            var activeOffers = await _offerRepository.GetActiveOffersAsync();
            return _mapper.Map<IEnumerable<OfferResponseDto>>(activeOffers);
        }

        public async Task<IEnumerable<OfferResponseDto>> GetOffersByPlaceIdAsync(Guid placeId)
        {
            var offers = await _offerRepository.GetOffersByPlaceIdAsync(placeId);
            return _mapper.Map<IEnumerable<OfferResponseDto>>(offers);
        }

        public async Task<OfferResponseDto> UpdateOfferAsync(Guid id, UpdateOfferDto dto)
        {
            // 1. التأكد إن العرض موجود أصلاً
            var existingOffer = await _offerRepository.GetByIdAsync(id);
            if (existingOffer == null)
                throw new KeyNotFoundException("عذراً، هذا العرض غير موجود.");

            // 2. تحديث البيانات (AutoMapper بيقوم بالواجب)
            _mapper.Map(dto, existingOffer);

            // 👈 5. رفع الصورة الجديدة وتحديث اللينك لو الموبايل/الويب بعت صورة جديدة
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                existingOffer.GalleryImages = await _imageService.UploadImageAsync(dto.ImageFile);
            }

            // 3. الحفظ في الداتابيز
            await _offerRepository.UpdateAsync(existingOffer);
            await _offerRepository.SaveChangesAsync();

            // 4. إرجاع النتيجة
            return _mapper.Map<OfferResponseDto>(existingOffer);
        }

        public async Task<bool> DeleteOfferAsync(Guid id)
        {
            // 1. التأكد إن العرض موجود
            var existingOffer = await _offerRepository.GetByIdAsync(id);
            if (existingOffer == null)
                throw new KeyNotFoundException("عذراً، هذا العرض غير موجود مسبقاً.");

            // 2. الحذف من الداتابيز
            await _offerRepository.DeleteAsync(existingOffer);
            await _offerRepository.SaveChangesAsync();

            return true;
        }

        public async Task ExpireFinishedOffersAsync()
        {
            // 1. نجيب كل العروض اللي وقتها خلص
            var expiredOffers = await _offerRepository.GetExpiredActiveOffersAsync(DateTime.UtcNow);

            // لو مفيش عروض خلصت، العسكري يروح ينام تاني 😴
            if (!expiredOffers.Any())
                return;

            // 2. نلف عليهم واحد واحد ونغير حالتهم
            foreach (var offer in expiredOffers)
            {
                offer.Status = OfferStatus.Expired;
                await _offerRepository.UpdateAsync(offer);
            }

            // 3. نحفظ التعديلات دي كلها في الداتابيز خبطة واحدة
            await _offerRepository.SaveChangesAsync();
        }

        /// <summary>
        /// جيب عرض واحد بـ ID
        /// </summary>
        public async Task<OfferResponseDto?> GetOfferByIdAsync(Guid offerId)
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null)
                return null;

            return _mapper.Map<OfferResponseDto>(offer);
        }
    }
}