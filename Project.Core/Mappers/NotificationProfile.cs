using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.Domain.Entities.NotificationPayload;
using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Project.Core.Mappers
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            // 1. من Request (DTO) -> Entity
            CreateMap<NotificationAddRequest, Notification>()
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => false))
                // التريكاية هنا: خد الـ Data وحولها لـ String JSON 👇
                .ForMember(dest => dest.Payload, opt => opt.MapFrom(src =>
                    src.Data != null ? JsonSerializer.Serialize(src.Data, (JsonSerializerOptions?)null) : null));

            // 2. من Entity -> Response (DTO)
            CreateMap<Notification, NotificationResponse>()
                // التريكاية هنا: خد الـ Payload (String) وحوله لـ Object 👇
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.Payload)
                    ? null
                    : JsonSerializer.Deserialize<NotificationData>(src.Payload, (JsonSerializerOptions?)null)));
        }
    }
}
