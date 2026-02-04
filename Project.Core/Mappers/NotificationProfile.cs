using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.Domain.Entities.NotificationPayload;
using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Project.Core.Mappers
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            // 1. من Request لـ Entity (وانت بتحفظ)
            CreateMap<NotificationAddRequest, Notification>()
                .ForMember(dest => dest.Payload, opt => opt.MapFrom(src =>
                    src.Data != null ? JsonSerializer.Serialize(src.Data, (JsonSerializerOptions?)null) : null)) // حول الـ Object لـ String
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message)); // لو الأسماء مختلفة

            // 2. من Entity لـ Response (وانت بتعرض)
            CreateMap<Notification, NotificationResponse>()
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message)) // لو سميتها Body في الـ Response
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.Payload)
                    ? JsonSerializer.Deserialize<NotificationData>(src.Payload, (JsonSerializerOptions?)null)
                    : null)); // حول الـ String لـ Object
        }
    }
}
