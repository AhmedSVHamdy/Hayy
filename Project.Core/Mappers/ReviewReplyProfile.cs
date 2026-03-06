using AutoMapper;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateReviewReplyDTO;

namespace Project.Core.Mappers
{
    public class ReviewReplyProfile : Profile
    {
        public ReviewReplyProfile()
        {
            CreateMap<CreateReviewReplyDto, ReviewReply>();
            CreateMap<ReviewReply, ReviewReplyResponseDto>();
        }
    }
}
