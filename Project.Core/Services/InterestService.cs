using Project.Core.Domain.Entities;
using Project.Core.Domain.RopositoryContracts;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Services
{
    public class InterestService : IInterestService
    {
        // لاحظ: لا يوجد DbContext هنا!
        private readonly ICategoryRepository _categoryRepo;
        private readonly IUserInterestRepository _interestRepo;

        public InterestService(ICategoryRepository categoryRepo, IUserInterestRepository interestRepo)
        {
            _categoryRepo = categoryRepo;
            _interestRepo = interestRepo;
        }

        public async Task<List<CategoryWithTagsDTO>> GetAllInterestsAsync()
        {
            // GetCategoris
            var categories = await _categoryRepo.GetAllWithTagsAsync();

            // 2. تحويلها لـ DTO (مسؤولية السيرفس)
            return categories.Select(c => new CategoryWithTagsDTO
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl,
                Tags = c.CategoryTags.Select(ct => new TagDTO
                {
                    Id = ct.Tag.Id,
                    Name = ct.Tag.Name
                }).ToList()
            }).ToList();
        }

        public async Task<bool> SaveUserInterestsAsync(Guid userId, UserInterestRequestDTO request)
        {
            var userInterests = new List<UserInterestProfile>();
            var timestamp = DateTime.UtcNow;

            // منطق الـ Skip
            bool isSkipped = (request.SelectedTagIds == null || !request.SelectedTagIds.Any()) &&
                             (request.SelectedCategoryIds == null || !request.SelectedCategoryIds.Any());

            if (isSkipped)
            {
                // نطلب من الريبوزيتوري يجيب لنا أول 3 كاتيجوريز (بدون ما نعرف هو جابهم ازاي)
                var defaultCategoryIds = await _categoryRepo.GetTopCategoryIdsAsync(3);

                foreach (var catId in defaultCategoryIds)
                {
                    userInterests.Add(new UserInterestProfile
                    {
                        UserId = userId,
                        CategoryId = catId,
                        InterestScore = 0.5m,
                        LastUpdated = timestamp
                    });
                }
            }
            else
            {
                // منطق الاختيار العادي
                if (request.SelectedTagIds != null)
                {
                    foreach (var tagId in request.SelectedTagIds)
                    {
                        userInterests.Add(new UserInterestProfile
                        {
                            UserId = userId,
                            TagId = tagId,
                            InterestScore = 1.0m,
                            LastUpdated = timestamp
                        });
                    }
                }

                if (request.SelectedCategoryIds != null)
                {
                    foreach (var catId in request.SelectedCategoryIds)
                    {
                        userInterests.Add(new UserInterestProfile
                        {
                            UserId = userId,
                            CategoryId = catId,
                            InterestScore = 0.8m,
                            LastUpdated = timestamp
                        });
                    }
                }
            }

            if (userInterests.Any())
            {
                // الحفظ عن طريق الريبوزيتوري
                await _interestRepo.AddRangeAsync(userInterests);
                await _interestRepo.SaveChangesAsync();
            }

            return true;
        }
    }
}
