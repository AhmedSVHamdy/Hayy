using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CeratePostComment;

namespace Project.Core.Services
{    
    public class PostCommentService : IPostCommentService
    {
        private readonly IPostCommentRepository _postCommentRepository;
        private readonly IBusinessPostRepository _businessPostRepository; // عشان نتأكد إن البوست موجود
        private readonly IMapper _mapper;
        private readonly INotifier _notifier;
        private readonly IUserLogService _userLogService;
        private readonly IPlaceRepository _placeRepository;
        public PostCommentService(IPostCommentRepository postCommentRepository, IBusinessPostRepository businessPostRepository, IMapper mapper, INotifier notifier, IUserLogService userLogService, IPlaceRepository placeRepository)
        {
            _postCommentRepository = postCommentRepository;
            _businessPostRepository = businessPostRepository;
            _mapper= mapper;
            _notifier = notifier;
            _userLogService = userLogService;
            _placeRepository = placeRepository;
        }
        public async Task<CommentResponseDto> AddCommentAsync(CreateCommentDto dto)
        {
            // 🛑 1. Business Validation: التحقق من وجود البوست (أول حاجة لازم تحصل)
            var post = await _businessPostRepository.GetPostByIdAsync(dto.PostId);
            if (post == null)
            {
                throw new KeyNotFoundException("عذراً، هذا البوست غير موجود أو تم حذفه! 🚫");
            }

            // 🛑 2. Business Validation: التحقق من الرد (Reply Logic)
            if (dto.ParentCommentId.HasValue)
            {
                var parentComment = await _postCommentRepository.GetCommentByIdAsync(dto.ParentCommentId.Value);

                // أ: هل الكومنت الأب موجود؟
                if (parentComment == null)
                {
                    throw new KeyNotFoundException("التعليق الذي تحاول الرد عليه غير موجود! 🚫");
                }

                // ب: (تريكة مهمة) هل الكومنت الأب ده تبع نفس البوست؟ 
                // عشان محدش يبعت PostId بتاع بوست، ويرد على كومنت في بوست تاني خالص!
                if (parentComment.PostId != dto.PostId)
                {
                    throw new ArgumentException("لا يمكنك الرد على تعليق ينتمي لبوست آخر! ⚠️");
                }
            }

            // ✅ 3. Mapping & Saving (دلوقتي بس نقدر نحفظ وإحنا مطمنين)
            var commentEntity = _mapper.Map<PostComment>(dto);

            // إضافة التاريخ والوقت (لو الـ Mapper مش بيعملها)
            commentEntity.CreatedAt = DateTime.UtcNow;

            var addedComment = await _postCommentRepository.AddCommentAsync(commentEntity);

            // 3. Notification Logic 🔔
            // محتاجين نعرف البوست ده تبع مطعم إيه عشان نبعت لصاحب المطعم
            // (ممكن تحتاج دالة في PostRepo تجيب البوست بالـ ID)
            // هنفترض إننا بنبعت للجروب بتاع البوست نفسه عشان لو فيه يوزرز متابعين البوست
            await _notifier.SendNotificationToUser(
                dto.PostId.ToString(),
                $"💬 تعليق جديد: {dto.Content}"
            );

            Guid categoryId = post.Place != null ? post.Place.CategoryId : Guid.Empty;
            // 4. Mongo Log (AI) 🧠
            var logDto = new CreateUserLogDto
            {
                UserId = dto.UserId,
                ActionType = ActionType.Comment, // تأكد إنها موجودة في الـ Enum (رقم 2)
                TargetType = TargetType.Post,    // ضيف Post في TargetType Enum
                TargetId = dto.PostId,
                Details = dto.Content,
                Duration = 0,
                CategoryId = categoryId,
            };
            await _userLogService.LogActivityAsync(logDto);

            // 5. Return
            return _mapper.Map<CommentResponseDto>(addedComment);

        }

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsByPostIdAsync(Guid postId)
        {
            var comments = await _postCommentRepository.GetCommentsByPostIdAsync(postId);
            return _mapper.Map<IEnumerable<CommentResponseDto>>(comments);
        }

        public async Task<PagedResult<CommentResponseDto>> GetCommentsByPostIdPagedAsync(Guid postId, int pageNumber, int pageSize)
        {
            // 1. هات الداتا
            var comments = await _postCommentRepository.GetCommentsByPostIdPagedAsync(postId, pageNumber, pageSize);

            // 2. هات العدد الكلي
            var totalCount = await _postCommentRepository.GetTotalCommentsCountAsync(postId);

            // 3. Map
            var dtos = _mapper.Map<List<CommentResponseDto>>(comments);

            // 4. Return Paged Result
            return new PagedResult<CommentResponseDto>(dtos, totalCount, pageNumber, pageSize);
        }
    }
}
