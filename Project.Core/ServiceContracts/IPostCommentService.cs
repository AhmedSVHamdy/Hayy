using Project.Core.Domain.Entities;
using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CeratePostComment;
namespace Project.Core.ServiceContracts
{
    public interface IPostCommentService
    {
        Task<CommentResponseDto> AddCommentAsync(CreateCommentDto createPostCommentDto);
        Task<IEnumerable<CommentResponseDto>> GetCommentsByPostIdAsync(Guid postId);
        Task<PagedResult<CommentResponseDto>> GetCommentsByPostIdPagedAsync(Guid postId, int pageNumber, int pageSize);
    }
}
