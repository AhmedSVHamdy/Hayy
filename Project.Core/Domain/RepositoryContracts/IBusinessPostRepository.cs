using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IBusinessPostRepository
    {
        Task<BusinessPost> AddPostAsync(BusinessPost post);
        Task<IEnumerable<BusinessPost>> GetPostsByPlaceIdAsync(Guid placeId);
        Task<BusinessPost?> GetPostByIdAsync(Guid postId);
    }
}
