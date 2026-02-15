using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Mappers
{
    public class ReviewProfile : Profile
    {
        public ReviewProfile()
        {
            // 1. من DTO لـ Entity (عشان الإضافة في SQL)
            CreateMap<CreateReviewDto, Review>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // 2. من Entity لـ Response DTO (عشان العرض)
            CreateMap<Review, ReviewResponseDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName));
            // ملحوظة: UserName هيشتغل بس لو عملت Include للـ User وأنت بتجيب الداتا من الريبو
        }
    }
}
