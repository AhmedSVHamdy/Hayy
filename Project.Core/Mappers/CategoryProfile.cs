using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.DTO.Categories;
using Project.Core.DTO.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Mappers
{
    public class CategoryProfile : Profile
    {

        public CategoryProfile()
        {
           

            // 1. Tags
            CreateMap<Tag, TagDto>();
            

            // 2. Categories
            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Category, CategoryWithTagsDto>()
                // السحر هنا: بنقوله هات الـ Tags من جدول العلاقة CategoryTags
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.CategoryTags.Select(ct => ct.Tag)));
        }
    }
}
