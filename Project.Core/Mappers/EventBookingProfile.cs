using AutoMapper;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateEventBooking;

namespace Project.Core.Mappers
{
    public class EventBookingProfile : Profile
    {
        public EventBookingProfile()
        {
            CreateMap<EventBooking, BookingResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.EventTitle, opt => opt.MapFrom(src => src.Event.Title))
                // بنحسب التكلفة الإجمالية بناءً على سعر الإيفنت وعدد التذاكر
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TicketQuantity * src.Event.Price));
        }
    }
}
