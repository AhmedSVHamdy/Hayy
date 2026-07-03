using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService; // ✅ ضيفنا ده

        public PlaceService(
            IPlaceRepository placeRepo,
            ICategoryRepository categoryRepo,
            IGenericRepository<Tag> tagRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IImageService imageService) // ✅ ضيفنا ده
        {
            _placeRepo = placeRepo;
            _categoryRepo = categoryRepo;
            _tagRepo = tagRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService; // ✅ ضيفنا ده
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
            place.IsActive = true;

            // ✅ 3. رفع الصورة لو موجودة
            if (dto.CoverImage != null)
                place.CoverImage = await _imageService.UploadImageAsync(dto.CoverImage);

            await _placeRepo.AddAsync(place);

            // 4. إضافة الوسوم (Tags Logic)
            if (dto.TagIds != null && dto.TagIds.Any())
            {
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

            // 5. الحفظ الفعلي في الداتابيز
            await _unitOfWork.SaveChangesAsync();

            // 6. الإرجاع
            var response = _mapper.Map<PlaceResponseDto>(place);
            response.CategoryName = category.Name;

            return response;
        }

        public async Task<IEnumerable<PlaceResponseDto>> GetPlacesByCategoryIdAsync(Guid categoryId)
        {
            var places = await _placeRepo.GetByCategoryIdAsync(categoryId);
            return _mapper.Map<IEnumerable<PlaceResponseDto>>(places);
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

        public async Task<List<PlaceResponseDto>> BasicSearchAsync(string searchTerm, Guid? categoryId)
        {
            var places = await _placeRepo.SearchPlacesAsync(searchTerm, categoryId);
            return _mapper.Map<List<PlaceResponseDto>>(places);
        }

        public async Task<IEnumerable<PlaceResponseDto>> GetPlacesByBusinessAsync(Guid businessId)
        {
            var places = await _placeRepo.GetByBusinessIdAsync(businessId);
            return _mapper.Map<IEnumerable<PlaceResponseDto>>(places);
        }

        public async Task<bool> DeletePlaceAsync(Guid placeId, Guid businessId)
        {
            var place = await _placeRepo.GetByIdWithDetailsForUpdateAsync(placeId);

            if (place == null)
                throw new Exception("المكان غير موجود");

            if (place.BusinessId != businessId)
                throw new UnauthorizedAccessException("مش مسموح تمسح مكان مش بتاعك");

            place.IsActive = false;
            _placeRepo.Update(place);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<PlaceResponseDto> UpdatePlaceAsync(Guid placeId, Guid businessId, UpdatePlaceDto dto)
        {
            var place = await _placeRepo.GetByIdWithDetailsForUpdateAsync(placeId);

            if (place == null)
                throw new Exception("المكان غير موجود");

            if (place.BusinessId != businessId)
                throw new UnauthorizedAccessException("مش مسموح تعدل مكان مش بتاعك");

            if (dto.Name != null) place.Name = dto.Name;
            if (dto.Description != null) place.Description = dto.Description;
            if (dto.Latitude.HasValue) place.Latitude = (decimal)dto.Latitude.Value;
            if (dto.Longitude.HasValue) place.Longitude = (decimal)dto.Longitude.Value;

            // ✅ رفع الصورة الجديدة لو بعتها
            if (dto.CoverImage != null)
                place.CoverImage = await _imageService.UploadImageAsync(dto.CoverImage);

            if (dto.CategoryId.HasValue)
            {
                var category = await _categoryRepo.GetByIdAsync(dto.CategoryId.Value);
                if (category == null) throw new Exception("التصنيف غير موجود");
                place.CategoryId = dto.CategoryId.Value;
            }

            if (dto.TagIds != null)
            {
                place.PlaceTags.Clear();
                foreach (var tagId in dto.TagIds)
                {
                    var tag = await _tagRepo.GetByIdAsync(tagId);
                    if (tag != null)
                        place.PlaceTags.Add(new PlaceTag { PlaceId = place.Id, TagId = tagId });
                }
            }

            if (dto.OpeningHours != null)
            {
                place.OpeningHours.Clear();
                var newHours = _mapper.Map<List<OpeningHour>>(dto.OpeningHours);
                foreach (var hour in newHours)
                {
                    hour.PlaceId = place.Id;
                    place.OpeningHours.Add(hour);
                }
            }

            _placeRepo.Update(place);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PlaceResponseDto>(place);
        }
    }
}