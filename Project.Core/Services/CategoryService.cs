using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO.Categories;
using Project.Core.DTO.Tags;
using Project.Core.ServiceContracts;

namespace Project.Core.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly IGenericRepository<Tag> _tagRepo;
        private readonly IGenericRepository<CategoryTag> _categoryTagRepo;
        private readonly IUnitOfWork _unitOfWork; // 👈 1. ضيفنا ده
        private readonly IMapper _mapper;

        public CategoryService(
            ICategoryRepository categoryRepo,
            IGenericRepository<Tag> tagRepo,
            IGenericRepository<CategoryTag> categoryTagRepo,
            IUnitOfWork unitOfWork, // 👈 2. حقناه هنا
            IMapper mapper)
        {
            _categoryRepo = categoryRepo;
            _tagRepo = tagRepo;
            _categoryTagRepo = categoryTagRepo;
            _unitOfWork = unitOfWork; // 👈 3. سجلناه
            _mapper = mapper;
        }

        public async Task<CategoryWithTagsDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            category.Id = Guid.NewGuid();

            // إضافة في الذاكرة
            await _categoryRepo.AddAsync(category);

            // 🛑 حفظ فعلي في الداتابيز
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CategoryWithTagsDto>(category);
        }

        public async Task<bool> AssignTagsToCategoryAsync(AssignTagsDto dto)
        {
            // 1. التأكد من وجود التصنيف
            var category = await _categoryRepo.GetByIdAsync(dto.CategoryId);
            if (category == null) throw new Exception("التصنيف غير موجود");

            bool hasChanges = false;

            foreach (var tagId in dto.TagIds)
            {
                // 2. التأكد من وجود الوسم
                var tag = await _tagRepo.GetByIdAsync(tagId);
                if (tag == null) continue;

                // 3. إنشاء العلاقة
                await _categoryTagRepo.AddAsync(new CategoryTag
                {
                    Id = Guid.NewGuid(),
                    CategoryId = dto.CategoryId,
                    TagId = tagId
                });

                hasChanges = true;
            }

            // 🛑 حفظ التغييرات مرة واحدة في الآخر (عشان الأداء)
            if (hasChanges)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }

        public async Task<CategoryWithTagsDto?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _categoryRepo.GetByIdWithTagsAsync(id);
            if (category == null) return null;

            return _mapper.Map<CategoryWithTagsDto>(category);
        }

        public async Task<IEnumerable<CategoryWithTagsDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryWithTagsDto>>(categories);
        }
    }
}