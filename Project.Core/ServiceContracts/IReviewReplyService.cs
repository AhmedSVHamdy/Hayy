using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateReviewReplyDTO;

namespace Project.Core.ServiceContracts
{
    public interface IReviewReplyService
    {
        Task<ReviewReplyResponseDto> AddReplyAsync(CreateReviewReplyDto dto);
        Task<PagedResult<ReviewReplyResponseDto>> GetRepliesByReviewIdAsync(Guid reviewId, int pageNumber, int pageSize);
        Task DeleteReplyAsync(Guid replyId, Guid userId); // userId عشان نتأكد إنه صاحب الرد
    }
}
