using System;
using System.Collections.Generic;
using System.Linq; // محتاجين دي عشان Any
using System.Threading.Tasks;
using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO.Places;
using Project.Core.ServiceContracts;

namespace Project.Core.Services
{
    public class PlaceService : IPlaceService
    {
        private readonly IPlaceRepository _placeRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IGenericRepository<Tag> _tagRepo;
        private readonly IUnitOfWork _unitOfWork; // 👈 1. ضيفنا ده
        private readonly IMapper _mapper;

        public PlaceService(
            IPlaceRepository placeRepo,
            ICategoryRepository categoryRepo,
            IGenericRepository<Tag> tagRepo,
            IUnitOfWork unitOfWork, // 👈 2. ضيفنا ده في الـ Constructor
            IMapper mapper)
        {
            _placeRepo = placeRepo;
            _categoryRepo = categoryRepo;
            _tagRepo = tagRepo;
            _unitOfWork = unitOfWork; // 👈 3. وسجلناه هنا
            _mapper = mapper;
        }

        public async Task<PlaceResponseDto> CreatePlaceAsync(CreatePlaceDto dto)
        {
            // 1. التحقق من وجود التصنيف
            var category = await _categoryRepo.GetByIdAsync(dto.CategoryId);
            if (category == null)
                throw new Exception("التصنيف غير موجود");

            // 2. التحويل
            var place = _mapper.Map<Place>(dto);
            place.Id = Guid.NewGuid();

            // 3. إضافة الوسوم (Tags Logic)
            if (dto.TagIds != null && dto.TagIds.Any())
            {
                // تحسين بسيط: بدل اللوب، ممكن نجيب التاجز الموجودة ونضيفها
                foreach (var tagId in dto.TagIds)
                {
                    var tag = await _tagRepo.GetByIdAsync(tagId);
                    if (tag != null)
                    {
                        place.PlaceTags.Add(new PlaceTag
                        {
                            PlaceId = place.Id,
                            TagId = tagId
                        });
                    }
                }
            }

            // 4. الإضافة (في الذاكرة فقط)
            await _placeRepo.AddAsync(place);

            // 5. الحفظ الفعلي في الداتابيز 🛑 (دي الخطوة اللي كانت ناقصة)
            // تأكد إن اسم الدالة عندك في IUnitOfWork هو CompleteAsync أو SaveChangesAsync
            await _unitOfWork.SaveChangesAsync();

            // 6. الإرجاع
            var response = _mapper.Map<PlaceResponseDto>(place);

            // تأكد إن البيانات دي راجعة صح
            response.CategoryName = category.Name;

            return response;
        }

        public async Task<PlaceResponseDto?> GetPlaceByIdAsync(Guid id)
        {
            var place = await _placeRepo.GetByIdWithDetailsAsync(id);
            if (place == null) return null;
            return _mapper.Map<PlaceResponseDto>(place);
        }

        public async Task<IEnumerable<PlaceResponseDto>> GetAllPlacesAsync()
        {
            var places = await _placeRepo.GetAllWithDetailsAsync();
            return _mapper.Map<IEnumerable<PlaceResponseDto>>(places);
        }
    }
}