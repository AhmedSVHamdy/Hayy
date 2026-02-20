using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CerateBusinessPostDto;

namespace Project.Core.ServiceContracts
{
    public interface IBusinessPostService
    {
        Task<PostResponseDto> CreatePostAsync(CreatePostDto createPostDto);
        Task<IEnumerable<PostResponseDto>> GetPostsByPlaceIdAsync(Guid placeId);

        Task<PagedResult<PostResponseDto>> GetPostsByPlaceIdPagedAsync(Guid placeId, int pageNumber, int pageSize);
    }
}
