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
            // Input -> Entity
            CreateMap<CreatePostDto, BusinessPost>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Entity -> Output
            CreateMap<BusinessPost, PostResponseDto>()
                .ForMember(dest => dest.PlaceName, opt => opt.MapFrom(src => src.Place.Name))
                .ForMember(dest => dest.PlaceImage, opt => opt.MapFrom(src => src.Place.GalleryImages)) // تأكد من اسم الخاصية في Place
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.PostLikes.Count)) // عد اللايكات
                .ForMember(dest => dest.CommentsCount, opt => opt.MapFrom(src => src.PostComments.Count)); // عد الكومنتات
        }
    }
}
