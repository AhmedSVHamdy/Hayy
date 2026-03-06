using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateOfferDTO;

namespace Project.Core.Mappers
{
    public class OfferProfile : Profile
    {
        public OfferProfile()
        {
            CreateMap<Offer, OfferResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateOfferDto, Offer>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OfferStatus.Active)); // العرض بينزل متفعل ديفولت

            CreateMap<UpdateOfferDto, Offer>();
        }
    }
}
