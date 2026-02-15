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

        // دالة عرض تقييمات مكان معين
        Task<IEnumerable<ReviewResponseDto>> GetReviewsByPlaceIdAsync(Guid placeId);
    }
}
