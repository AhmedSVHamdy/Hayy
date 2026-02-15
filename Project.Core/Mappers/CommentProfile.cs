using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CeratePostComment;


namespace Project.Core.Mappers
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            // Input -> Entity
            CreateMap<CeratePostComment,PostComment>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            // Entity -> Output
            CreateMap<PostComment, CommentResponseDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName)) // تأكد من اسم الخاصية في User
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies)); // الردود (لو عايز تضيفها في الـ DTO)
                //.ForMember(dest => dest.UserImage, opt => opt.MapFrom(src => src.User.UserImage));
        }
    }
}
