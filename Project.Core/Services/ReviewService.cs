using AutoMapper;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository; // مسؤول الـ SQL
        private readonly IUserLogService _userLogService;     // مسؤول الـ MongoDB
        private readonly IMapper _mapper;                     // مسؤول التحويل
        private readonly INotifier _notifier; // SignalR
        private readonly IPlaceRepository _placeRepo;         // Place (عشان نحدث التقييم)
        private readonly IUserInterestRepository _interestRepository;




        public ReviewService(IReviewRepository reviewRepository, IUserLogService userLogService, IMapper mapper , INotifier notifier, IPlaceRepository placeRepo, IUserInterestRepository interestRepository)
        {
            _reviewRepository = reviewRepository;
            _userLogService = userLogService;
            _mapper = mapper;
            _notifier = notifier;
            _placeRepo = placeRepo;
            _interestRepository = interestRepository;
        }

        public async Task<ReviewResponseDto> AddReviewAsync(CreateReviewDto createReviewDto)
        {
            // 🛑 Business Validation 1: هل المكان موجود؟
            var place = await _placeRepo.GetByIdWithDetailsAsync(createReviewDto.PlaceId);
            if (place == null)
            {
                // بنرمي Exception والكنترولر هيصطاده ويرجع 404 أو 400
                throw new KeyNotFoundException("عذراً، هذا المكان غير موجود في قاعدة البيانات.");
            }

            // 🛑 Business Validation 2: هل المستخدم قيم المكان ده قبل كده؟
            var alreadyReviewed = await _reviewRepository.HasUserReviewedPlaceAsync(createReviewDto.UserId, createReviewDto.PlaceId);
            if (alreadyReviewed)
            {
                throw new InvalidOperationException("لقد قمت بتقييم هذا المكان مسبقاً، لا يمكنك التقييم مرتين.");
            }


            // 1️⃣ Mapping: تحويل الـ DTO القادم من المستخدم إلى Entity للداتا بيز
            var reviewEntity = _mapper.Map<Review>(createReviewDto);

            // 2️⃣ SQL: الحفظ في قاعدة البيانات الأساسية
            var addedReview = await _reviewRepository.AddReviewAsync(reviewEntity);

            // 2️⃣ Update Place Rating (تحديث متوسط التقييم)
            // شيلنا الكومنت وكده الكود شغال لأن _placeRepo موجود
            await _placeRepo.UpdatePlaceRatingAsync(createReviewDto.PlaceId);

            // 4️⃣ SignalR: تنبيه صاحب المطعم فوراً 🔔
            // بنفترض إن صاحب المطعم عامل Join لجروب بنفس الـ PlaceId
            if (place != null)
            {
                // 2. نجهز اسم الجروب الخاص بإدارة المكان ده
                string groupName = $"Management_{place.Id}";

                // 3. نبعت الإشعار للجروب كله (لأن معندناش يوزر محدد)
                await _notifier.SendNotificationToGroup(
                    groupName,
                    $"في ريفيو جديد {createReviewDto.Rating} نجوم لمكانك! ⭐: {createReviewDto.Comment}"
                );
            }

            var userInterests = await _interestRepository.GetUserInterestsByUserIdAsync(createReviewDto.UserId);

            Guid? userTopCategoryId = userInterests
                .OrderByDescending(i => i.InterestScore)
                .FirstOrDefault(i => i.CategoryId.HasValue)?.CategoryId;

            List<Guid> userTagIds = userInterests
                .Where(i => i.TagId.HasValue)
                .Select(i => i.TagId.Value)
                .ToList();
            // 3️⃣ MongoDB: تسجيل الحدث للـ AI والتحليلات 🧠
            var logDto = new CreateUserLogDto
            {
                UserId = createReviewDto.UserId,
                ActionType = ActionType.Review,   // نوع العملية: تقييم
                TargetType = TargetType.Place,    // الهدف: مكان
                TargetId = createReviewDto.PlaceId,
                CategoryId = place.CategoryId, // ✅ جبنا الـ CategoryId من المكان اللي بحثنا عنه فوق
                TagId = place.PlaceTags?.Select(t => t.TagId).ToList() ?? new List<Guid>(),
                Details = createReviewDto.Comment, // (اختياري) ممكن نخزن الكومنت هنا للتحليل
                UserTopInterestCategoryId = userTopCategoryId,
                UserInterestTagIds = userTagIds
            };

            // نبعت اللوج للمونجو
            await _userLogService.LogActivityAsync(logDto);

            
            return _mapper.Map<ReviewResponseDto>(addedReview);
        }

        public async Task<PagedResult<ReviewResponseDto>> GetReviewsByPlaceIdPagedAsync(Guid placeId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            // أ) هات الداتا (ReviewRepository لازم يكون فيه دالة Paged)
            // (مطلوب منك تضيف GetReviewsPagedAsync في الريبوزيتوري زي ما عملنا في النوتفكيشن)
            var reviews = await _reviewRepository.GetReviewsPagedAsync(placeId, pageNumber, pageSize);

            // ب) هات العدد الكلي
            var totalCount = await _reviewRepository.GetCountByPlaceIdAsync(placeId);

            // ج) التحويل والرد
            var dtos = _mapper.Map<List<ReviewResponseDto>>(reviews);
            return new PagedResult<ReviewResponseDto>(dtos, totalCount, pageNumber, pageSize);
        }
        public async Task<ReviewResponseDto> UpdateReviewAsync(Guid reviewId, UpdateReviewDto dto, Guid userId)
        {
            // 1. هل التقييم موجود؟
            var review = await _reviewRepository.GetReviewByIdAsync(reviewId);
            if (review == null)
            {
                throw new KeyNotFoundException("عذراً، هذا التقييم غير موجود.");
            }

            // 2. 🛡️ حماية: التأكد إن اليوزر اللي بيعدل هو صاحب التقييم
            if (review.UserId != userId)
            {
                throw new UnauthorizedAccessException("غير مصرح لك بتعديل تقييم لا يخصك.");
            }

            // 3. تحديث البيانات
            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.ReviewImages = dto.ReviewImages;

            // 4. الحفظ في الداتابيز
            await _reviewRepository.UpdateAsync(review);

            // 5. إرجاع النتيجة
            return _mapper.Map<ReviewResponseDto>(review);
        }
    }
}
