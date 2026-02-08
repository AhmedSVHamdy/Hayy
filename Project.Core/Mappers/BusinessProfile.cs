using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using System.Linq; // 👈 مهمة جداً عشان نقدر نستخدم OrderByDescending

namespace Project.Core.Mappers
{
    public class BusinessProfile : Profile
    {
        public BusinessProfile()
        {
            // =========================================================
            // 1. From Entity TO Response (Get Data)
            // =========================================================
            CreateMap<Business, BusinessResponse>()
                // أ. تحويل الـ Enum لنص
                .ForMember(dest => dest.VerificationStatus, opt => opt.MapFrom(src => src.VerificationStatus.ToString()))

                // ب. اسم المالك (User.FullName) مع فحص Null
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.FullName : "Unknown"))

                // ج. تاريخ المراجعة (VerifiedAt)
                // ⚠️ التصحيح: لازم نجيب أحدث عملية توثيق من القائمة
                .ForMember(dest => dest.VerifiedAt, opt => opt.MapFrom(src =>
                    src.Verifications != null && src.Verifications.Any()
                        ? src.Verifications.OrderByDescending(v => v.ReviewedAt).FirstOrDefault().ReviewedAt
                        : null));


            // =========================================================
            // 2. From DTO TO Entity (Create Data - Onboarding)
            // =========================================================
            CreateMap<BusinessOnboardingDTO, Business>()
                // نتجاهل الحقول اللي بتتولد أوتوماتيك
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.VerificationStatus, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }
}