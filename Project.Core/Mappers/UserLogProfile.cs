using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Mappers
{
    public class UserLogProfile : Profile
    {
        public UserLogProfile()
        {
            // 1. من DTO لـ Entity (عشان الإضافة)
            CreateMap<CreateUserLogDto, UserLog>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            
        }
    }
}
