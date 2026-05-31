using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CerateBusinessPostDto;
using static Project.Core.DTO.CeratePostComment;

namespace Project.Core.ServiceContracts
{
    public interface IBusinessPostService
    {
        Task<PostResponseDto> CreatePostAsync(CreatePostDto createPostDto);
        Task<IEnumerable<PostResponseDto>> GetPostsByPlaceIdAsync(Guid placeId);
        Task<PagedResult<PostResponseDto>> GetPostsByPlaceIdPagedAsync(Guid placeId, int pageNumber, int pageSize);
        Task<PagedResult<PostResponseDto>> GetAllPostsPagedAsync(int pageNumber, int pageSize);

        Task<PostResponseDto> UpdatePostAsync(Guid postId, UpdatePostDto dto, Guid userId);
        Task DeletePostAsync(Guid postId, Guid userId);

        // 👈 جديد: جيب التعليقات على البوست
        Task<IEnumerable<CommentResponseDto>> GetPostCommentsAsync(Guid postId);

        // 👈 جديد: الرد على تعليق
        Task<CommentResponseDto> ReplyToCommentAsync(ReplyCommentDto dto);
    }
}
