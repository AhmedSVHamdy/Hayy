using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Mappers
{
    public class BusinessMappingProfile : Profile
    {
        public BusinessMappingProfile()
        {
            // التحويل من الـ DTO اللي جاي من الفرونت لجدول البيزنس
            CreateMap<BusinessOnboardingDTO, Business>()
                // 👇 أهم جزء: بنقول للمابر "طنش" ملفات الصور لأننا بنرفعها يدوي
                .ForMember(dest => dest.LogoImage, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            // 2. باقي الصور (TaxCard, Identity, etc) مش محتاجين نعملهم Ignore 
            // لأنهم أصلاً مش موجودين في كلاس Business فـ AutoMapper هيطنشهم لوحده
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
