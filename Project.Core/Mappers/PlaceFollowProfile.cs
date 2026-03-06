using AutoMapper;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CeratePlaceFollow;

namespace Project.Core.Mappers
{
    public class PlaceFollowProfile : Profile
    {
        public PlaceFollowProfile()
        {
            CreateMap<PlaceFollow, PlaceFollowResponseDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.PlaceName, opt => opt.MapFrom(src => src.Place.Name)); // تأكد إن اسم خاصية الاسم في Place هي Name
        }
    }
}
