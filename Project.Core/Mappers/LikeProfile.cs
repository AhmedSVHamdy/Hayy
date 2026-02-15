using AutoMapper;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CeratePostLike;

namespace Project.Core.Mappers
{
    public class LikeProfile : Profile
    {
        public LikeProfile() 
        {
            CreateMap<ToggleLikeDto, PostLike>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid())); // بنعمل ID جديد
        }
    }
}
