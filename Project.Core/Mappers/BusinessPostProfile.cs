using AutoMapper;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CerateBusinessPostDto;

namespace Project.Core.Mappers
{
    public class BusinessPostProfile : Profile
    {
        public BusinessPostProfile()
        {
            CreateMap<CreatePostDto, BusinessPost>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Entity -> Output
            CreateMap<BusinessPost, PostResponseDto>()
                .ForMember(dest => dest.PlaceName, opt => opt.MapFrom(src => src.Place.Name))

// 1️⃣ حل مشكلة صورة المكان (بناخد أول صورة لو هي لستة، أو استبدلها بـ LogoUrl لو عندك)
                    .ForMember(dest => dest.PlaceImage, opt => opt.MapFrom(src => src.Place.GalleryImages ?? string.Empty))
                // 2️⃣ حماية من الـ Null عشان الكود مايضربش لو مفيش تفاعل
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.PostLikes != null ? src.PostLikes.Count : 0)) 
                .ForMember(dest => dest.CommentsCount, opt => opt.MapFrom(src => src.PostComments != null ? src.PostComments.Count : 0));
        }
    }
}
