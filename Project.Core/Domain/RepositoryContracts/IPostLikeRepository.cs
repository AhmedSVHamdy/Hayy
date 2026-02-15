using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IPostLikeRepository
    {
        // دالة بتدور هل اليوزر ده عمل لايك للبوست ده قبل كدة؟
        Task<PostLike?> GetLikeAsync(Guid userId, Guid postId);

        Task AddLikeAsync(PostLike like);
        Task RemoveLikeAsync(PostLike like);

        // دالة تجيب عدد اللايكات للبوست
        Task<int> GetLikesCountAsync(Guid postId);
    }
}
