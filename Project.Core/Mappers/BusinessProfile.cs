using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Mappers
{
    public class BusinessProfile : Profile
    {
        public BusinessProfile()
        {
            CreateMap<Business, BusinessResponse>()
                // 1. تحويل الـ Enum لنص
                .ForMember(dest => dest.VerificationStatus, opt => opt.MapFrom(src => src.VerificationStatus.ToString()))

                // 2. اسم المالك (User.FullName)
                // تأكد إن User فيه FullName، لو مفيش استخدم UserName
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "Unknown"))

                // 3. 👇 التصحيح هنا: استخدمنا ReviewedAt
                .ForMember(dest => dest.VerifiedAt, opt => opt.MapFrom(src =>
                    src.BusinessVerifications != null ? (DateTime?)src.BusinessVerifications.ReviewedAt : null));
        }
    }
}
