using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CerateBusinessPostDto;

namespace Project.Core.Services
{
    public class BusinessPostService : IBusinessPostService
    {
        private readonly IBusinessPostRepository _postRepository;
        private readonly IUserLogService _userLogService; // Mongo
        private readonly IMapper _mapper;
        private readonly INotifier _notifier; // SignalR
       // private readonly IPlaceRepository _placeRepository; // 1️⃣ ضفنا الريبو ده عشان نتأكد من المكان
        public BusinessPostService(IBusinessPostRepository postRepository, IUserLogService userLogService, IMapper mapper, INotifier notifier)//IPlaceRepository placeRepository
        {
            _postRepository = postRepository;
            _userLogService = userLogService;
            _mapper = mapper;
            _notifier = notifier;
           // _placeRepository = placeRepository;
        }

        public async Task<PostResponseDto> CreatePostAsync(CreatePostDto dto)
        {

            // 🛑 Business Validation 1: هل المكان موجود؟
            //var place = await _placeRepository.GetPlaceByIdAsync(dto.PlaceId);

            //if (place == null)
            //{
            //    throw new KeyNotFoundException("عذراً، هذا المكان غير موجود 🚫");
            //}

            // 🛑 Business Validation 2: (أخطر واحد) هل اليوزر هو صاحب المكان؟
            // لازم نتأكد إن الـ User اللي باعت الريكويست هو نفسه الـ OwnerId بتاع المكان
            //if (place.OwnerId != dto.UserId)
            //{
            //    throw new UnauthorizedAccessException("غير مسموح لك بالنشر باسم هذا المكان! أنت لست المالك 👮‍♂️");
            //}

            // 🛑 Business Validation 3: (اختياري) هل المكان مفعل؟
            // لو المكان واخد بان أو لسه تحت المراجعة، مينفعش ينزل بوستات
            /*
            if (!place.IsActive)
            {
                throw new InvalidOperationException("هذا المكان غير مفعل حالياً ولا يمكنه النشر.");
            }
            */



            // 1. Mapping
            var postEntity = _mapper.Map<BusinessPost>(dto);

            // 2. Save to SQL
            var addedPost = await _postRepository.AddPostAsync(postEntity);

            // 3. SignalR Notification 🔔
            // بنبعت تنبيه لكل الناس اللي عاملين Follow للمكان ده (Group = PlaceId)
            await _notifier.SendNotificationToGroup(
                dto.PlaceId.ToString(),
                $"بوست جديد من مطعمك المفضل! 🍔: {dto.Content}"
            );

            // 4. Mongo Log (AI) 🧠
            // بنسجل إن المكان ده نزل بوست، عشان الـ AI يعرف إن المكان ده نشيط (Active)
            var logDto = new CreateUserLogDto
            {

                // UserId هنا ممكن يكون الـ OwnerId لو معاك، أو نسيبه Null لو العملية باسم السيستم
                UserId = dto.UserId,
                ActionType = ActionType.Post, // ضيف Post في الـ Enum
                TargetType = TargetType.Place,
                TargetId = dto.PlaceId,
                //CategoryId = place.CategoryId,
                SearchQuery = dto.Content, // نخزن محتوى البوست للتحليل
                Duration = 0
            };
            await _userLogService.LogActivityAsync(logDto);

            // 5. Return
            return _mapper.Map<PostResponseDto>(addedPost);
        }

        public async Task<IEnumerable<PostResponseDto>> GetPostsByPlaceIdAsync(Guid placeId)
        {
            var posts = await _postRepository.GetPostsByPlaceIdAsync(placeId);
            return _mapper.Map<IEnumerable<PostResponseDto>>(posts);
        }
    }
}
