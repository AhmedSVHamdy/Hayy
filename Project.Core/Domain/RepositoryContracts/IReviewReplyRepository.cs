using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IReviewReplyRepository
    {
        Task<ReviewReply> AddAsync(ReviewReply reply);
        Task<ReviewReply?> GetByIdAsync(Guid id);

        // Pagination هنا
        Task<List<ReviewReply>> GetRepliesByReviewIdPagedAsync(Guid reviewId, int pageNumber, int pageSize);
        Task<int> GetTotalCountByReviewIdAsync(Guid reviewId);

        Task DeleteAsync(ReviewReply reply);
    }
}
