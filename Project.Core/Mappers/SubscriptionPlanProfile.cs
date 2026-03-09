using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.DTO.Plans;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Mappers
{
    public class SubscriptionPlanProfile : Profile
    {
        public SubscriptionPlanProfile()
        {
            // Entity → ResponseDto
            CreateMap<SubscriptionPlan, SubscriptionPlanResponseDto>();

            // AddRequestDto → Entity
            // بنتجاهل الـ Id و IsActive عشان بنعملهم يدوياً في الـ Service
            CreateMap<AddSubscriptionPlanRequestDto, SubscriptionPlan>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.BusinessPlans, opt => opt.Ignore());

            // UpdateRequestDto → Entity (بنعمل Map على الـ Entity الموجودة)
            CreateMap<UpdateSubscriptionPlanRequestDto, SubscriptionPlan>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.BusinessPlans, opt => opt.Ignore());
        }
    }
}
