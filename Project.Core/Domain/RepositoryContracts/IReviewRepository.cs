using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IReviewRepository
    {
        Task<Review> AddReviewAsync(Review review);
        // ممكن تضيف دوال تانية زي GetReviewsByPlaceId
        Task<IEnumerable<Review>> GetReviewsByPlaceIdAsync(Guid placeId);

        // دي وظيفتها ترجع true لو اليوزر قيم المكان ده قبل كده
        Task<bool> HasUserReviewedPlaceAsync(Guid userId, Guid placeId);
    }
}
