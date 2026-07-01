using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Project.Core.DTO.CeratePlaceFollow;

namespace Project.Core.Services
{
    public class PlaceFollowService : IPlaceFollowService
    {
        private readonly IPlaceFollowRepository _placeFollowRepository; // مسؤول الـ SQL
        private readonly IUserLogService _userLogService;               // مسؤول الـ MongoDB
        private readonly IMapper _mapper;                               // مسؤول التحويل
        private readonly INotifier _notifier;                           // SignalR
        private readonly IPlaceRepository _placeRepo;                   // Place (عشان نتأكد من المكان ونجيب الكاتيجوري)
        private readonly IUserInterestRepository _interestRepository;
        private readonly IBusinessRepository _businessRepo;             // مسؤول الـ Analytics

        public PlaceFollowService(
            IPlaceFollowRepository placeFollowRepository,
            IUserLogService userLogService,
            IMapper mapper,
            INotifier notifier,
            IPlaceRepository placeRepo,
            IUserInterestRepository interestRepository,
            IBusinessRepository businessRepo)
        {
            _placeFollowRepository = placeFollowRepository;
            _userLogService = userLogService;
            _mapper = mapper;
            _notifier = notifier;
            _placeRepo = placeRepo;
            _interestRepository = interestRepository;
            _businessRepo = businessRepo;
        }

        public async Task<bool> ToggleFollowAsync(Guid userId, TogglePlaceFollowDto dto)
        {
            // 🛑 Business Validation 1: هل المكان موجود؟
            var place = await _placeRepo.GetByIdWithDetailsAsync(dto.PlaceId);
            if (place == null)
            {
                throw new KeyNotFoundException("عذراً، هذا المكان غير موجود في قاعدة البيانات.");
            }

            // 🛑 Business Validation 2: هل المستخدم متابع المكان ده؟
            var existingFollow = await _placeFollowRepository.GetFollowAsync(userId, dto.PlaceId);

            if (existingFollow != null)
            {
                // ❌ إلغاء المتابعة (Unfollow)
                await _placeFollowRepository.RemoveAsync(existingFollow);

                // ✅ تحديث Analytics: تقليل عدد المتابعين بـ 1
                await UpdateBusinessFollowersCountAsync(place.BusinessId, -1);

                return false;
            }
            else
            {
                // ✅ إضافة متابعة جديدة (Follow)
                var newFollow = new PlaceFollow
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PlaceId = dto.PlaceId,
                    CreatedAt = DateTime.UtcNow
                };

                // 1️⃣ SQL: الحفظ في قاعدة البيانات الأساسية
                await _placeFollowRepository.AddAsync(newFollow);

                // ✅ تحديث Analytics: زيادة عدد المتابعين بـ 1
                await UpdateBusinessFollowersCountAsync(place.BusinessId, 1);

                // 2️⃣ SignalR: تنبيه صاحب المطعم فوراً 🔔
                string groupName = $"Management_{dto.PlaceId}";
                await _notifier.SendNotificationToGroup(
                    groupName,
                    "يوجد مستخدم جديد قام بمتابعة مكانك! 👤"
                );


                var userInterests = await _interestRepository.GetUserInterestsByUserIdAsync(userId);
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
                    UserId = userId,
                    ActionType = ActionType.Follow,
                    TargetType = TargetType.Place,
                    TargetId = dto.PlaceId,
                    CategoryId = place.CategoryId,
                    Details = "قام بمتابعة المكان",
                    TagId = place.PlaceTags?.Select(t => t.TagId).ToList() ?? new List<Guid>(),
                    UserTopInterestCategoryId = userTopCategoryId,
                    UserInterestTagIds = userTagIds
                };

                // نبعت اللوج للمونجو
                await _userLogService.LogActivityAsync(logDto);

                return true;
            }
        }

        // 1. دالة جلب كل المتابعين لمكان معين (Followers)
        public async Task<PagedResult<PlaceFollowResponseDto>> GetFollowersByPlaceIdPagedAsync(Guid placeId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var (items, totalCount) = await _placeFollowRepository.GetFollowersByPlaceIdAsync(placeId, pageNumber, pageSize);

            var dtos = _mapper.Map<List<PlaceFollowResponseDto>>(items);
            return new PagedResult<PlaceFollowResponseDto>(dtos, totalCount, pageNumber, pageSize);
        }

        // 2. دالة جلب كل الأماكن اللي اليوزر بيتابعها (Followed Places)
        public async Task<PagedResult<PlaceFollowResponseDto>> GetFollowedPlacesByUserIdPagedAsync(Guid userId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var (items, totalCount) = await _placeFollowRepository.GetFollowedPlacesByUserIdAsync(userId, pageNumber, pageSize);

            var dtos = _mapper.Map<List<PlaceFollowResponseDto>>(items);
            return new PagedResult<PlaceFollowResponseDto>(dtos, totalCount, pageNumber, pageSize);
        }

        // =========================================================
        // ✅ 3. دالة مساعدة (Helper Method) لتحديث الإحصائيات بأمان 
        // =========================================================
        private async Task UpdateBusinessFollowersCountAsync(Guid businessId, int amountChange)
        {
            // نجلب البيزنس مع الـ Analytics الخاصة به
            var business = await _businessRepo.GetBusinessByIdAsync(businessId);

            if (business != null)
            {
                if (business.BusinessAnalytics != null)
                {
                    // لو السجل موجود، بنزود أو ننقص حسب القيمة (amountChange)
                    // نستخدم Math.Max عشان العدد مستحيل يقل عن الصفر بالغلط
                    business.BusinessAnalytics.TotalFollowers = Math.Max(0, business.BusinessAnalytics.TotalFollowers + amountChange);
                    business.BusinessAnalytics.LastUpdated = DateTime.UtcNow;
                }
                else if (amountChange > 0)
                {
                    // لو السجل مش موجود أصلاً (نادرة الحدوث بسبب المزامنة)، نعمله من الصفر
                    var newAnalytics = new BusinessAnalytic
                    {
                        Id = Guid.NewGuid(),
                        BusinessId = businessId,
                        TotalFollowers = 1,
                        TotalReviews = 0,
                        TotalViews = 0,
                        AvgRating = 0,
                        MonthlyRevenue = 0,
                        LastUpdated = DateTime.UtcNow
                    };
                    await _businessRepo.AddBusinessAnalyticAsync(newAnalytics);
                }

                // حفظ التغييرات في قاعدة البيانات
                await _businessRepo.SaveChangesAsync();
            }
        }
    }
}