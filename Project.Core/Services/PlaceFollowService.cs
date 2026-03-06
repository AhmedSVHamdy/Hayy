using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CeratePlaceFollow;

namespace Project.Core.Services
{
    public class PlaceFollowService : IPlaceFollowService
    {
        private readonly IPlaceFollowRepository _placeFollowRepository; // مسؤول الـ SQL
        private readonly IUserLogService _userLogService;       // مسؤول الـ MongoDB
        private readonly IMapper _mapper;                       // مسؤول التحويل
        private readonly INotifier _notifier;                   // SignalR
        private readonly IPlaceRepository _placeRepo;           // Place (عشان نتأكد من المكان ونجيب الكاتيجوري)

        public PlaceFollowService(
            IPlaceFollowRepository placeFollowRepository,
            IUserLogService userLogService,
            IMapper mapper,
            INotifier notifier,
            IPlaceRepository placeRepo)
        {
            _placeFollowRepository = placeFollowRepository;
            _userLogService = userLogService;
            _mapper = mapper;
            _notifier = notifier;
            _placeRepo = placeRepo;
        }

        public async Task<bool> ToggleFollowAsync(Guid userId, TogglePlaceFollowDto dto)
        {
            // 🛑 Business Validation 1: هل المكان موجود؟
            // استخدم الميثود اللي بتجيب المكان عندك (ممكن تكون GetByIdAsync أو GetByIdWithDetailsAsync)
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
                // (مطلوب منك تتأكد إن الريبوزيتوري فيه دالة RemoveAsync أو بتعملها Remove و Save جواها)
                await _placeFollowRepository.RemoveAsync(existingFollow);
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

                // 2️⃣ SignalR: تنبيه صاحب المطعم فوراً 🔔
                string groupName = $"Management_{dto.PlaceId}";
                await _notifier.SendNotificationToGroup(
                    dto.PlaceId.ToString(),
                    "يوجد مستخدم جديد قام بمتابعة مكانك! 👤"
                );

                // 3️⃣ MongoDB: تسجيل الحدث للـ AI والتحليلات 🧠
                var logDto = new CreateUserLogDto
                {
                    UserId = userId,
                    ActionType = ActionType.Follow,   // نوع العملية: متابعة (ضيفها في الـ Enum)
                    TargetType = TargetType.Place,    // الهدف: مكان
                    TargetId = dto.PlaceId,
                    CategoryId = place.CategoryId,    // ✅ جبنا الـ CategoryId من المكان اللي بحثنا عنه فوق
                    Details = "قام بمتابعة المكان",   // نص يوضح الحدث
                    Duration = 0
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
    }
}
