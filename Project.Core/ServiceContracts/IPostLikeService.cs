using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CeratePostLike;

namespace Project.Core.ServiceContracts
{
    public interface IPostLikeService
    {
        Task<LikeResponseDto> ToggleLikeAsync(ToggleLikeDto toggleLikeDto);
    }
}
