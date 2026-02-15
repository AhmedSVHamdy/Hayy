using AutoMapper;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.SignalR;
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
        //private readonly IPlaceRepository _placeRepo;         // Place (عشان نحدث التقييم)
        


        public ReviewService(IReviewRepository reviewRepository, IUserLogService userLogService, IMapper mapper , INotifier notifier)//IPlaceRepository placeRepo
        {
            _reviewRepository = reviewRepository;
            _userLogService = userLogService;
            _mapper = mapper;
            _notifier = notifier;
            //_placeRepo = placeRepo;

        }

        public async Task<ReviewResponseDto> AddReviewAsync(CreateReviewDto createReviewDto)
        {
            // 🛑 Business Validation 1: هل المكان موجود؟
            //var place = await _placeRepo.GetPlaceByIdAsync(createReviewDto.PlaceId);
            //if (place == null)
            //{
            //    // بنرمي Exception والكنترولر هيصطاده ويرجع 404 أو 400
            //    throw new KeyNotFoundException("عذراً، هذا المكان غير موجود في قاعدة البيانات.");
            //}

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
            //await _placeRepo.UpdatePlaceRatingAsync(createReviewDto.PlaceId);

            // 4️⃣ SignalR: تنبيه صاحب المطعم فوراً 🔔
            // بنفترض إن صاحب المطعم عامل Join لجروب بنفس الـ PlaceId
            await _notifier.SendNotificationToGroup(
                createReviewDto.PlaceId.ToString(),
                $"في ريفيو جديد {createReviewDto.Rating} نجوم! ⭐"
            );



            // 3️⃣ MongoDB: تسجيل الحدث للـ AI والتحليلات 🧠
            var logDto = new CreateUserLogDto
            {
                UserId = createReviewDto.UserId,
                ActionType = ActionType.Review,   // نوع العملية: تقييم
                TargetType = TargetType.Place,    // الهدف: مكان
                TargetId = createReviewDto.PlaceId,
                //CategoryId = place.CategoryId, // ✅ جبنا الـ CategoryId من المكان اللي بحثنا عنه فوق
                SearchQuery = createReviewDto.Comment, // (اختياري) ممكن نخزن الكومنت هنا للتحليل
                Duration = 0,
            };

            // نبعت اللوج للمونجو
            await _userLogService.LogActivityAsync(logDto);

            
            return _mapper.Map<ReviewResponseDto>(addedReview);
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByPlaceIdAsync(Guid placeId)
        {
            // 1. نجيب البيانات من SQL
            var reviews = await _reviewRepository.GetReviewsByPlaceIdAsync(placeId);

            // 2. نحولها لـ List of DTOs ونرجعها
            return _mapper.Map<IEnumerable<ReviewResponseDto>>(reviews);
        }
    }
}
