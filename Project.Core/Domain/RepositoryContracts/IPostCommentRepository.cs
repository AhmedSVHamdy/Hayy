using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Project.Core.Domain.Entities;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IPostCommentRepository
    {
        Task<PostComment> AddCommentAsync(PostComment comment);
        // الدالة دي هتجيب الكومنتات الرئيسية فقط (والردود هتيجي جواها)
        Task<IEnumerable<PostComment>> GetCommentsByPostIdAsync(Guid postId);

        Task<PostComment?> GetCommentByIdAsync(Guid commentId);

    }
}
