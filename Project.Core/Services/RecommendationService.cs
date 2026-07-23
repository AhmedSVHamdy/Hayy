
using Microsoft.Extensions.Configuration;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using static Project.Core.DTO.RecommendedItemDto;

namespace Project.Core.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public RecommendationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        /// <summary>
        /// جلب التوصيات من الـ AI API (Hugging Face)
        /// </summary>
        public async Task<IEnumerable<RecommendedItemDto>> GetUserRecommendationsAsync(Guid userId)
        {
            // 1. قراءة رابط الـ API من الإعدادات
            var baseUrl = _configuration["AiSettings:BaseUrl"];

            // لو الرابط مش موجود في الـ appsettings
            if (string.IsNullOrEmpty(baseUrl))
                throw new Exception("AI BaseUrl is missing in appsettings.json");

            var url = $"{baseUrl}/recommendations/{userId}";

            // 2. إرسال الطلب لبايثون
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // 3. تحويل الـ JSON اللي راجع لكائنات C#
                var result = JsonSerializer.Deserialize<AiRecommendationResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data ?? new List<RecommendedItemDto>();
            }

            // لو حصل مشكلة في الاتصال يرجع لستة فاضية
            return new List<RecommendedItemDto>();
        }
    }
}