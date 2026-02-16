using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.DTO.Places;
namespace Project.Core.Mappers
{
    

    public class PlaceProfile : Profile
    {
        public PlaceProfile()
        {
            // 1. Create Map (Input -> Entity)
            CreateMap<CreatePlaceDto, Place>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PlaceTags, opt => opt.Ignore()); // هنتعامل معاها يدوي

            CreateMap<OpeningHourDto, OpeningHour>();

            // 2. Response Map (Entity -> Output)
            CreateMap<Place, PlaceResponseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.PlaceTags.Select(pt => pt.Tag)));

            CreateMap<OpeningHour, OpeningHourDto>();
        }
    }
}
