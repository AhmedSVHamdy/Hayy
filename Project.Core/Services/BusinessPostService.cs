using AutoMapper;
using Hangfire;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Project.Core.DTO.CerateBusinessPostDto;
using static Project.Core.DTO.CeratePostComment; // 👈 السطر ده اللي حل مشكلة الـ Interface

namespace Project.Core.Services
{
    public class BusinessPostService : IBusinessPostService
    {
        private readonly IBusinessPostRepository _postRepository;
        private readonly IPostCommentRepository _commentRepository;
        private readonly IMapper _mapper;
        private readonly INotifier _notifier;
        private readonly IPlaceRepository _placeRepository;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IImageService _imageService;

        public BusinessPostService(
            IBusinessPostRepository postRepository,
            IPostCommentRepository commentRepository,
            IMapper mapper,
            INotifier notifier,
            IPlaceRepository placeRepository,
            IBackgroundJobClient backgroundJobClient,
            IImageService imageService)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _mapper = mapper;
            _notifier = notifier;
            _placeRepository = placeRepository;
            _backgroundJobClient = backgroundJobClient;
            _imageService = imageService;
        }

        public async Task<PostResponseDto> CreatePostAsync(CreatePostDto dto)
        {
            var place = await _placeRepository.GetByIdWithDetailsAsync(dto.PlaceId);

            if (place == null)
            {
                throw new KeyNotFoundException("عذراً، هذا المكان غير موجود 🚫");
            }

            if (!place.IsActive)
            {
                throw new InvalidOperationException("هذا المكان غير مفعل حالياً ولا يمكنه النشر.");
            }

            var postEntity = _mapper.Map<BusinessPost>(dto);

            // رفع الصورة لو الموبايل/الويب بعت ملف
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                postEntity.PostAttachments = await _imageService.UploadImageAsync(dto.ImageFile);
            }

            var addedPost = await _postRepository.AddPostAsync(postEntity);

            string title = $"تحديث جديد من {place.Name} 🍔";
            string msg = dto.Content.Length > 50 ? dto.Content.Substring(0, 50) + "..." : dto.Content;

            // SignalR Notification
            await _notifier.NotifyFollowersRealtimeAsync(
                dto.PlaceId,
                title,
                msg,
                addedPost.Id.ToString(),
                ReferenceType.Post.ToString(),
                NotificationType.PostAlert.ToString()
            );

            // Hangfire Background Job
            _backgroundJobClient.Enqueue<INotificationService>(service =>
                service.NotifyFollowersBackgroundJobAsync(
                    dto.PlaceId,
                    title,
                    msg,
                    addedPost.Id.ToString(),
                    ReferenceType.Post.ToString(),
                    NotificationType.PostAlert.ToString()
                )
            );

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
            if (pageSize <= 0) pageSize = 50;
            if (pageSize > 50) pageSize = 50;

            var posts = await _postRepository.GetPostsByPlaceIdPagedAsync(placeId, pageNumber, pageSize);
            var totalCount = await _postRepository.GetCountByPlaceIdAsync(placeId);
            var dtos = _mapper.Map<List<PostResponseDto>>(posts);

            return new PagedResult<PostResponseDto>(dtos, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<PostResponseDto>> GetAllPostsPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 50;
            if (pageSize > 50) pageSize = 50;

            var posts = await _postRepository.GetAllPostsPagedAsync(pageNumber, pageSize);
            var totalCount = await _postRepository.GetTotalCountAsync();
            var dtos = _mapper.Map<List<PostResponseDto>>(posts);

            return new PagedResult<PostResponseDto>(dtos, totalCount, pageNumber, pageSize);
        }

        public async Task<PostResponseDto> UpdatePostAsync(Guid postId, UpdatePostDto dto, Guid userId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new KeyNotFoundException("عذراً، هذا المنشور غير موجود.");
            }

            var place = await _placeRepository.GetByIdAsync(post.PlaceId);
            if (place == null)
            {
                throw new KeyNotFoundException("عذراً، المكان المرتبط بهذا المنشور غير موجود.");
            }

            post.Content = dto.Content;

            // تحديث الصورة لو بعت صورة جديدة
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                post.PostAttachments = await _imageService.UploadImageAsync(dto.ImageFile);
            }

            await _postRepository.UpdateAsync(post);

            return _mapper.Map<PostResponseDto>(post);
        }

        public async Task DeletePostAsync(Guid postId, Guid userId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new KeyNotFoundException("عذراً، هذا المنشور غير موجود مسبقاً.");
            }

            var place = await _placeRepository.GetByIdAsync(post.PlaceId);
            if (place == null)
            {
                throw new KeyNotFoundException("عذراً، المكان المرتبط بهذا المنشور غير موجود.");
            }

            await _postRepository.DeleteAsync(post);
        }

        // 👇 مطابقة للـ Interface بالمللي 👇

        public async Task<IEnumerable<CommentResponseDto>> GetPostCommentsAsync(Guid postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
                throw new ArgumentException("البوست غير موجود.");

            var comments = await _commentRepository.GetCommentsByPostIdAsync(postId);
            return _mapper.Map<IEnumerable<CommentResponseDto>>(comments);
        }

        public async Task<CommentResponseDto> ReplyToCommentAsync(ReplyCommentDto dto)
        {
            var parentComment = await _commentRepository.GetCommentByIdAsync(dto.CommentId);
            if (parentComment == null)
                throw new ArgumentException("التعليق المراد الرد عليه غير موجود.");

            var post = await _postRepository.GetPostByIdAsync(parentComment.PostId);
            if (post == null)
                throw new ArgumentException("البوست غير موجود.");

            var reply = new PostComment
            {
                Id = Guid.NewGuid(),
                PostId = parentComment.PostId,
                UserId = dto.UserId,
                ParentCommentId = dto.CommentId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddCommentAsync(reply);
            return _mapper.Map<CommentResponseDto>(reply);
        }

        public async Task<PostResponseDto?> GetPostByIdAsync(Guid postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
                return null;

            return _mapper.Map<PostResponseDto>(post);
        }
    }
}