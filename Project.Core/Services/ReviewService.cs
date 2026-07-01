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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Core.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository; // مسؤول الـ SQL
        private readonly IUserLogService _userLogService;     // مسؤول الـ MongoDB
        private readonly IMapper _mapper;                     // مسؤول التحويل
        private readonly INotifier _notifier;                 // SignalR
        private readonly IPlaceRepository _placeRepo;         // Place (عشان نحدث التقييم)
        private readonly IUserInterestRepository _interestRepository;
        private readonly IBusinessRepository _businessRepo;   // ✅ 1. مسؤول الـ Analytics

        public ReviewService(
            IReviewRepository reviewRepository,
            IUserLogService userLogService,
            IMapper mapper,
            INotifier notifier,
            IPlaceRepository placeRepo,
            IUserInterestRepository interestRepository,
            IBusinessRepository businessRepo) // ✅ حقن الريبوزيتوري هنا
        {
            _reviewRepository = reviewRepository;
            _userLogService = userLogService;
            _mapper = mapper;
            _notifier = notifier;
            _placeRepo = placeRepo;
            _interestRepository = interestRepository;
            _businessRepo = businessRepo;
        }

        public async Task<ReviewResponseDto> AddReviewAsync(CreateReviewDto createReviewDto)
        {
            // 🛑 Business Validation 1: هل المكان موجود؟
            var place = await _placeRepo.GetByIdWithDetailsAsync(createReviewDto.PlaceId);
            if (place == null)
            {
                throw new KeyNotFoundException("عذراً، هذا المكان غير موجود في قاعدة البيانات.");
            }

            // 🛑 Business Validation 2: هل المستخدم قيم المكان ده قبل كده؟
            var alreadyReviewed = await _reviewRepository.HasUserReviewedPlaceAsync(createReviewDto.UserId, createReviewDto.PlaceId);
            if (alreadyReviewed)
            {
                throw new InvalidOperationException("لقد قمت بتقييم هذا المكان مسبقاً، لا يمكنك التقييم مرتين.");
            }

            // 1️⃣ Mapping
            var reviewEntity = _mapper.Map<Review>(createReviewDto);

            // 2️⃣ SQL: الحفظ في قاعدة البيانات الأساسية
            var addedReview = await _reviewRepository.AddReviewAsync(reviewEntity);

            // ✅ 3️⃣ Update Analytics: تحديث إحصائيات البيزنس (زيادة عدد التقييمات وتعديل المتوسط)
            await UpdateBusinessAnalyticsOnNewReviewAsync(place.BusinessId, createReviewDto.Rating);

            // 4️⃣ Update Place Rating (تحديث متوسط تقييم المكان نفسه)
            await _placeRepo.UpdatePlaceRatingAsync(createReviewDto.PlaceId);

            // 5️⃣ SignalR: تنبيه صاحب المطعم فوراً 🔔
            string groupName = $"Management_{place.Id}";
            await _notifier.SendNotificationToGroup(
                groupName,
                $"في ريفيو جديد {createReviewDto.Rating} نجوم لمكانك! ⭐: {createReviewDto.Comment}"
            );

            var userInterests = await _interestRepository.GetUserInterestsByUserIdAsync(createReviewDto.UserId);
            Guid? userTopCategoryId = userInterests
                .OrderByDescending(i => i.InterestScore)
                .FirstOrDefault(i => i.CategoryId.HasValue)?.CategoryId;

            List<Guid> userTagIds = userInterests
                .Where(i => i.TagId.HasValue)
                .Select(i => i.TagId.Value)
                .ToList();

            // 6️⃣ MongoDB: تسجيل الحدث للـ AI والتحليلات 🧠
            var logDto = new CreateUserLogDto
            {
                UserId = createReviewDto.UserId,
                ActionType = ActionType.Review,
                TargetType = TargetType.Place,
                TargetId = createReviewDto.PlaceId,
                CategoryId = place.CategoryId,
                TagId = place.PlaceTags?.Select(t => t.TagId).ToList() ?? new List<Guid>(),
                Details = createReviewDto.Rating.ToString(),
                UserTopInterestCategoryId = userTopCategoryId,
                UserInterestTagIds = userTagIds
            };

            await _userLogService.LogActivityAsync(logDto);

            return _mapper.Map<ReviewResponseDto>(addedReview);
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

            // ✅ نحتفظ بالتقييم القديم قبل التعديل لكي نضبط الإحصائيات
            decimal oldRating = review.Rating;

            // 3. تحديث البيانات
            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.ReviewImages = dto.ReviewImages;

            // 4. الحفظ في الداتابيز
            await _reviewRepository.UpdateAsync(review);

            // ✅ 5. تحديث Analytics: تعديل المتوسط القديم بالمتوسط الجديد
            var place = await _placeRepo.GetByIdWithDetailsAsync(review.PlaceId);
            if (place != null && oldRating != dto.Rating)
            {
                await UpdateBusinessAnalyticsOnEditReviewAsync(place.BusinessId, oldRating, dto.Rating);
                await _placeRepo.UpdatePlaceRatingAsync(review.PlaceId); // لا ننسى تحديث المكان نفسه
            }

            // 6. إرجاع النتيجة
            return _mapper.Map<ReviewResponseDto>(review);
        }

<<<<<<< HEAD
        /// <summary>
        /// جلب ريفيوهات اليوزر بالصفحات
        /// </summary>
=======

        // =========================================================
        // جلب تقييمات مستخدم معين (Pagination)
        // =========================================================
>>>>>>> f5dbb6814e90fa833b1f53379108b44cb1dea255
        public async Task<PagedResult<ReviewResponseDto>> GetReviewsByUserIdPagedAsync(Guid userId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

<<<<<<< HEAD
            // أ) هات ريفيوهات اليوزر
            var reviews = await _reviewRepository.GetReviewsByUserIdPagedAsync(userId, pageNumber, pageSize);

            // ب) هات العدد الكلي
            var totalCount = await _reviewRepository.GetCountByUserIdAsync(userId);

            // ج) التحويل والرد
            var dtos = _mapper.Map<List<ReviewResponseDto>>(reviews);
            return new PagedResult<ReviewResponseDto>(dtos, totalCount, pageNumber, pageSize);
        }
=======
            // جلب التقييمات الخاصة باليوزر من الـ Repository
            var reviews = await _reviewRepository.GetReviewsByUserIdPagedAsync(userId, pageNumber, pageSize);

            // جلب العدد الكلي لتقييمات هذا اليوزر
            var totalCount = await _reviewRepository.GetCountByUserIdAsync(userId);

            // التحويل للـ DTO وإرجاع النتيجة
            var dtos = _mapper.Map<List<ReviewResponseDto>>(reviews);
            return new PagedResult<ReviewResponseDto>(dtos, totalCount, pageNumber, pageSize);
        }
        public async Task<PagedResult<ReviewResponseDto>> GetReviewsByPlaceIdPagedAsync(Guid placeId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var reviews = await _reviewRepository.GetReviewsPagedAsync(placeId, pageNumber, pageSize);
            var totalCount = await _reviewRepository.GetCountByPlaceIdAsync(placeId);

            var dtos = _mapper.Map<List<ReviewResponseDto>>(reviews);
            return new PagedResult<ReviewResponseDto>(dtos, totalCount, pageNumber, pageSize);
        }


        // =========================================================
        // ✅ دوال مساعدة (Helper Methods) لتحديث الإحصائيات رياضياً 🧮
        // =========================================================

        // 1. عند إضافة تقييم جديد
        private async Task UpdateBusinessAnalyticsOnNewReviewAsync(Guid businessId, decimal newRating)
        {
            var business = await _businessRepo.GetBusinessByIdAsync(businessId);
            if (business != null)
            {
                var analytics = business.BusinessAnalytics;
                if (analytics != null)
                {
                    // الحسبة الرياضية للمتوسط: (المتوسط القديم * العدد القديم + التقييم الجديد) / العدد الجديد
                    decimal oldTotal = analytics.TotalReviews;
                    decimal oldAvg = analytics.AvgRating;

                    analytics.TotalReviews += 1;
                    analytics.AvgRating = Math.Round(((oldAvg * oldTotal) + newRating) / analytics.TotalReviews, 2);
                    analytics.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    // لو أول تقييم للإطلاق
                    analytics = new BusinessAnalytic
                    {
                        Id = Guid.NewGuid(),
                        BusinessId = businessId,
                        TotalReviews = 1,
                        AvgRating = Math.Round(newRating, 2),
                        TotalFollowers = 0,
                        TotalViews = 0,
                        MonthlyRevenue = 0,
                        LastUpdated = DateTime.UtcNow
                    };
                    await _businessRepo.AddBusinessAnalyticAsync(analytics);
                }
                await _businessRepo.SaveChangesAsync();
            }
        }

        // 2. عند تعديل تقييم قديم
        private async Task UpdateBusinessAnalyticsOnEditReviewAsync(Guid businessId, decimal oldRating, decimal newRating)
        {
            var business = await _businessRepo.GetBusinessByIdAsync(businessId);
            if (business != null && business.BusinessAnalytics != null)
            {
                var analytics = business.BusinessAnalytics;
                if (analytics.TotalReviews > 0)
                {
                    // نخصم التقييم القديم من المجموع الكلي، ونضيف التقييم الجديد، ثم نقسم على العدد
                    decimal currentTotalScore = analytics.AvgRating * analytics.TotalReviews;
                    decimal newTotalScore = currentTotalScore - oldRating + newRating;

                    analytics.AvgRating = Math.Round(newTotalScore / analytics.TotalReviews, 2);
                    analytics.LastUpdated = DateTime.UtcNow;

                    await _businessRepo.SaveChangesAsync();
                }
            }
        }
>>>>>>> f5dbb6814e90fa833b1f53379108b44cb1dea255
    }
}