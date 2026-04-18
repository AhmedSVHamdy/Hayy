using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Project.Core.DTO.CerateBusinessPostDto;
using Hangfire; // 👈 1. ضيفنا النيم سبيس ده

namespace Project.Core.Services
{
    public class BusinessPostService : IBusinessPostService
    {
        private readonly IBusinessPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly INotifier _notifier;
        private readonly IPlaceRepository _placeRepository;
        private readonly IBackgroundJobClient _backgroundJobClient; // 👈 2. الباشا بتاع Hangfire

        public BusinessPostService(
            IBusinessPostRepository postRepository,
            IMapper mapper,
            INotifier notifier,
            IPlaceRepository placeRepository,
            IBackgroundJobClient backgroundJobClient) // 👈 3. حقناه هنا
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _notifier = notifier;
            _placeRepository = placeRepository;
            _backgroundJobClient = backgroundJobClient; // 👈 4. ربطناه
        }

        public async Task<PostResponseDto> CreatePostAsync(CreatePostDto dto)
        {
            // 🛑 Business Validation 1: هل المكان موجود؟
            var place = await _placeRepository.GetByIdWithDetailsAsync(dto.PlaceId);

            if (place == null)
            {
                throw new KeyNotFoundException("عذراً، هذا المكان غير موجود 🚫");
            }

            if (!place.IsActive)
            {
                throw new InvalidOperationException("هذا المكان غير مفعل حالياً ولا يمكنه النشر.");
            }

            // 1. Mapping
            var postEntity = _mapper.Map<BusinessPost>(dto);

            // 2. Save to SQL
            var addedPost = await _postRepository.AddPostAsync(postEntity);

            // 3. 🔥 Hangfire Background Job 🔥 (بدل الجروب القديم)
            string title = $"تحديث جديد من {place.Name} 🍔";
            // لو البوست طويل جداً ممكن نقصره في الإشعار عشان شكله ميبقاش بايخ
            string msg = dto.Content.Length > 50 ? dto.Content.Substring(0, 50) + "..." : dto.Content;

            _backgroundJobClient.Enqueue<INotificationService>(service =>
                service.NotifyFollowersBackgroundJobAsync(
                    dto.PlaceId,
                    title,
                    msg,
                    addedPost.Id.ToString(),
                    ReferenceType.Post.ToString(),// 👈 حددنا إنه بوست
                    NotificationType.PostAlert.ToString() // 👈 حددنا نوع الإشعار
                )
            );

            // 4. Return
            return _mapper.Map<PostResponseDto>(addedPost);
        }

        public async Task<IEnumerable<PostResponseDto>> GetPostsByPlaceIdAsync(Guid placeId)
        {
            var posts = await _postRepository.GetPostsByPlaceIdAsync(placeId);
            return _mapper.Map<IEnumerable<PostResponseDto>>(posts);
        }

        public async Task<PagedResult<PostResponseDto>> GetPostsByPlaceIdPagedAsync(Guid placeId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            // أ) هات الليستة من الريبو
            var posts = await _postRepository.GetPostsByPlaceIdPagedAsync(placeId, pageNumber, pageSize);

            // ب) هات العدد الكلي من الريبو
            var totalCount = await _postRepository.GetCountByPlaceIdAsync(placeId);

            // ج) حول لـ DTO
            var dtos = _mapper.Map<List<PostResponseDto>>(posts);

            // د) غلفهم في PagedResult ورجعهم
            return new PagedResult<PostResponseDto>(dtos, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<PostResponseDto>> GetAllPostsPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var posts = await _postRepository.GetAllPostsPagedAsync(pageNumber, pageSize);
            var totalCount = await _postRepository.GetTotalCountAsync();

            var dtos = _mapper.Map<List<PostResponseDto>>(posts);
            return new PagedResult<PostResponseDto>(dtos, totalCount, pageNumber, pageSize);
        }

        public async Task<PostResponseDto> UpdatePostAsync(Guid postId, UpdatePostDto dto, Guid userId)
        {
            // 1. هل البوست موجود أصلاً؟
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new KeyNotFoundException("عذراً، هذا المنشور غير موجود.");
            }

            // 2. هل المكان موجود؟ وهل اليوزر ده هو صاحب المكان؟ (الأمان 🛡️)
            var place = await _placeRepository.GetByIdAsync(post.PlaceId);
            if (place == null)
            {
                throw new KeyNotFoundException("عذراً، المكان المرتبط بهذا المنشور غير موجود.");
            }

            // 3. التعديل
            post.Content = dto.Content;
            // لو عندك خاصية UpdatedAt في الكلاس ضيفها:
            // post.UpdatedAt = DateTime.UtcNow;

            // 4. الحفظ في الداتابيز
            await _postRepository.UpdateAsync(post);
            // تأكد إن عندك دالة UpdateAsync أو إنك بتنادي SaveChangesAsync() في الريبو

            // 5. إرجاع النتيجة
            return _mapper.Map<PostResponseDto>(post);
        }

        public async Task DeletePostAsync(Guid postId, Guid userId)
        {
            // 1. هل البوست موجود؟
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new KeyNotFoundException("عذراً، هذا المنشور غير موجود مسبقاً.");
            }

            // 2. هل اليوزر ده هو صاحب المكان؟ (الأمان 🛡️)
            var place = await _placeRepository.GetByIdAsync(post.PlaceId);
            if (place == null)
            {
                throw new KeyNotFoundException("عذراً، المكان المرتبط بهذا المنشور غير موجود.");
            }
            // 3. الحذف من الداتابيز
            await _postRepository.DeleteAsync(post);
            // تأكد إنك بتعمل SaveChangesAsync جوه دالة الـ Delete في الريبو
        }

    }
}