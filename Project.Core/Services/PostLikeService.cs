using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CeratePostLike;


namespace Project.Core.Services
{
    public class PostLikeService : IPostLikeService
    {
        private readonly IPostLikeRepository _postLikeRepository;
        private readonly IBusinessPostRepository _businessPostRepository;
        private readonly IMapper _mapper;
        private readonly INotifier _notifier;
        private readonly IUserLogService _userLogService; // Mongo
        private readonly IPlaceRepository _placeRepository;

        public PostLikeService(IPostLikeRepository postLikeRepository, IBusinessPostRepository businessPostRepository, IMapper mapper, INotifier notifier, IUserLogService userLogService, IPlaceRepository placeRepository)
        {
            _postLikeRepository = postLikeRepository;
            _businessPostRepository = businessPostRepository;
            _mapper = mapper;
            _notifier = notifier;
            _userLogService = userLogService;
            _placeRepository = placeRepository;
        }
        public async Task<LikeResponseDto> ToggleLikeAsync(ToggleLikeDto dto)
        {
            // 1. هل البوست ده موجود أصلاً؟ (Validation Logic)
            var post = await _businessPostRepository.GetPostByIdAsync(dto.PostId);
            if (post == null)
                throw new KeyNotFoundException("البوست ده مش موجود! 🤷‍♂️");

            // 2. هل اليوزر عمل لايك قبل كده؟
            var existingLike = await _postLikeRepository.GetLikeAsync(dto.UserId, dto.PostId);
            bool isLikedNow;

            if (existingLike != null)
            {
                // 🛑 موجود -> شيله (Unlike)
                await _postLikeRepository.RemoveLikeAsync(existingLike);
                isLikedNow = false;
            }
            else
            {
                // ✅ مش موجود -> ضيفه (Like)
                var newLike = _mapper.Map<PostLike>(dto);
                await _postLikeRepository.AddLikeAsync(newLike);
                isLikedNow = true;

                // 🔔 SignalR: تنبيه لأصحاب المكان (الجروب)
                // بنبعت للجروب اللي اسمه هو نفس الـ PlaceId بتاع البوست
                await _notifier.SendNotificationToUser(
                    post.PlaceId.ToString(),
                    $"❤️ إعجاب جديد على البوست بتاعك!"
                );

                // 🧠 Mongo Log: تسجيل الحدث للذكاء الاصطناعي
                // حماية: لو المكان مش جاي مع البوست، حط CategoryId بـ Empty عشان السيرفر ميقعش
                Guid categoryId = post.Place?.CategoryId ?? Guid.Empty;
                var logDto = new CreateUserLogDto
                {
                    UserId = dto.UserId,
                    ActionType = ActionType.Like,
                    TargetType = TargetType.Post,
                    TargetId = dto.PostId,
                    CategoryId = categoryId, // ممكن تجيبه من الـ Place لو عايز دقة أكتر
                    Duration = 0
                };
                await _userLogService.LogActivityAsync(logDto);
            }

            // 3. هات العدد الجديد للايكات عشان نحدث الفرونت
            int newCount = await _postLikeRepository.GetLikesCountAsync(dto.PostId);

            // 4. رجع النتيجة
            return new LikeResponseDto
            {
                IsLiked = isLikedNow,
                LikesCount = newCount
            };
        }
    }
    
}
