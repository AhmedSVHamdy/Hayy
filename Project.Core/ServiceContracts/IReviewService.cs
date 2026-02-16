using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface IReviewService
    {
        // دالة إضافة تقييم (بتاخد DTO وترجع DTO بالبيانات الجديدة زي الـ ID)
        Task<ReviewResponseDto> AddReviewAsync(CreateReviewDto createReviewDto);
        // بنغير الـ Return Type لـ PagedResult وبناخد PageNumber, PageSize
        Task<PagedResult<ReviewResponseDto>> GetReviewsByPlaceIdPagedAsync(Guid placeId, int pageNumber, int pageSize);
    }
}
