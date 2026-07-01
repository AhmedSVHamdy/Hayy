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
            // ==========================================
            // 1. من جدول البيزنس إلى الـ DTO (جلب البيانات)
            // ==========================================
            CreateMap<Business, BusinessProfileDTO>()
                .ForMember(dest => dest.VerificationStatus, opt => opt.MapFrom(src => src.VerificationStatus.ToString()))
                .ForMember(dest => dest.RejectionReason, opt => opt.Ignore());

            // ==========================================
            // 2. من الـ DTO لجدول البيزنس (إنشاء أو تحديث)
            // ==========================================
            CreateMap<BusinessOnboardingDTO, Business>()
                // نتجاهل المعرفات والصور والخصائص التي يتم معالجتها يدوياً في السيرفس
                .ForMember(dest => dest.LogoImage, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.VerificationStatus, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Verifications, opt => opt.Ignore())
                .ForMember(dest => dest.Places, opt => opt.Ignore())
                .ForMember(dest => dest.BusinessPlans, opt => opt.Ignore())
                .ForMember(dest => dest.Subscriptions, opt => opt.Ignore())
                // ✅ أمنية إضافية: منع نقل أي حقل مبعوث بقيمة فارغة (null) من الفرونت إند
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<BusinessAnalytic, BusinessAnalyticDTO>();
        }
    }


}
