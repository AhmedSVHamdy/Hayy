using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateReviewReplyDTO;

namespace Project.Core.Services
{
    public class ReviewReplyService : IReviewReplyService
    {
        private readonly IReviewReplyRepository _replyRepo;
        private readonly IReviewRepository _reviewRepo;
        private readonly INotifier _notifier;
        private readonly IMapper _mapper;
        private readonly IUserLogService _userLogService;

        public ReviewReplyService(IReviewReplyRepository replyRepo, IMapper mapper, IReviewRepository reviewRepo, INotifier notifier, IUserLogService userLogService)
        {
            _replyRepo = replyRepo;
            _mapper = mapper;
            _reviewRepo = reviewRepo;
            _notifier = notifier;
            _userLogService = userLogService;
        }

        public async Task<ReviewReplyResponseDto> AddReplyAsync(CreateReviewReplyDto dto)
        {
            // 1. هات الريفيو الأصلي (عشان نتأكد إنه موجود + نعرف مين صاحبه)
            var review = await _reviewRepo.GetReviewByIdAsync(dto.ReviewId);

            if (review == null)
                throw new KeyNotFoundException("الريفيو غير موجود!");

            // 2. الحفظ في الداتابيز
            var entity = _mapper.Map<ReviewReply>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.ReplierId = dto.ReplierId ?? Guid.Empty;

            var savedReply = await _replyRepo.AddAsync(entity);

            // 🚀 3. SignalR: ابعت إشعار لصاحب الريفيو
            // بنقوله: "فيه رد جديد جالك"
            if (review.UserId != Guid.Empty) // تأمين
            {
                await _notifier.SendNotificationToUser(
                    review.UserId.ToString(), // ده الـ ID بتاع العميل
                    $"💬 رد جديد على تعليقك: {dto.ReplyText}"
                );
            }

            try
            {
                // حماية: نتأكد إن Place مش null
                Guid categoryId = review.Place != null ? review.Place.CategoryId : Guid.Empty;

                var logDto = new CreateUserLogDto
                {
                    UserId = dto.ReplierId ?? Guid.Empty, // مين اللي رد؟
                    ActionType = ActionType.Reply,        // نوع الأكشن
                    TargetType = TargetType.Review,       // الرد كان على إيه؟
                    TargetId = dto.ReviewId,              // رقم الريفيو
                    CategoryId = categoryId,              // تصنيف المطعم (إيطالي، شامي...)
                    Details = dto.ReplyText,          // ممكن نخزن نص الرد عشان تحليل المشاعر (Sentiment Analysis)
                    Duration = 0
                };

                // بنستخدم Fire and Forget (مش بنستنى النتيجة عشان منأخرش الرد على اليوزر)
                await _userLogService.LogActivityAsync(logDto);
            }
            catch (Exception)
            {
                // لو اللوج فشل، مش لازم نوقف العملية كلها.. كمل عادي
                // (ممكن تعمل Logger.LogError هنا لو عايز)
            }

            return _mapper.Map<ReviewReplyResponseDto>(savedReply);
        }

        public async Task<PagedResult<ReviewReplyResponseDto>> GetRepliesByReviewIdAsync(Guid reviewId, int pageNumber, int pageSize)
        {
            var replies = await _replyRepo.GetRepliesByReviewIdPagedAsync(reviewId, pageNumber, pageSize);
            var totalCount = await _replyRepo.GetTotalCountByReviewIdAsync(reviewId);

            var dtos = _mapper.Map<List<ReviewReplyResponseDto>>(replies);

            return new PagedResult<ReviewReplyResponseDto>(dtos, totalCount, pageNumber, pageSize);
        }

        public async Task DeleteReplyAsync(Guid replyId, Guid userId)
        {
            var reply = await _replyRepo.GetByIdAsync(replyId);
            if (reply == null) throw new KeyNotFoundException("الرد غير موجود.");

            // Validation: هل هو صاحب الرد؟
            if (reply.ReplierId != userId)
                throw new UnauthorizedAccessException("غير مسموح لك بحذف هذا الرد.");

            await _replyRepo.DeleteAsync(reply);
        }
    }
}
