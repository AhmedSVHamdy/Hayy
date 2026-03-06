using AutoMapper;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateEventDTO;

namespace Project.Core.Mappers
{
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            CreateMap<EventCreateDto, Event>();

            CreateMap<Event, EventResponseDto>()
                // 1. تحويل الحالة لـ String
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))

                // 2. حساب التذاكر المحجوزة
                .ForMember(dest => dest.BookedTickets, opt => opt.MapFrom(src => src.EventBookings.Count()))

                // 3. حساب حالة قائمة الانتظار (لو التذاكر خلصت ولسه في مكان في الويت ليست)
                .ForMember(dest => dest.CanJoinWaitlist, opt => opt.MapFrom(src =>
                    src.IsWaitlistEnabled && (src.EventBookings.Count() - src.Capacity) < src.WaitlistLimit));
        }
    }
}
