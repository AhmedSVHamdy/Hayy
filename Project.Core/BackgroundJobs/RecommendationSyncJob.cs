using Hangfire;
using Project.Core.ServiceContracts;
using System;
using System.Threading.Tasks;

namespace Project.Core.BackgroundJobs
{
    public class RecommendationSyncJob
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationSyncJob(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        /// <summary>
        /// Synchronizes the user's recommendations with the frontend asynchronously.
        /// </summary>
        /// <remarks>This method retrieves the latest recommendations for the specified user and makes
        /// them available to the frontend. If an error occurs during synchronization, the exception is propagated to
        /// the caller.</remarks>
        /// <param name="userId">The unique identifier of the user whose recommendations are to be synchronized.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JobDisplayName("Sync Recommendations to Frontend")]
        public async Task SyncRecommendationsAsync(Guid userId)
        {
            try
            {
                // 1. جيب التوصيات من MongoDB
                var recommendations = await _recommendationService.GetUserRecommendationsAsync(userId);

                // 2. كل اللي بتحتاجه هو إن السيرفس بتاعك تعمل جيب للتوصيات
                //    والـ Endpoint يعرضها
                // (مفيش حاجة إضافية محتاجة هنا - التوصيات جاهزة!)

                // 3. لوج الـ Success
                Console.WriteLine($"✅ Recommendations synced for user {userId}. Total: {recommendations}");
            }
            catch (Exception ex)
            {
                // لوج الخطأ
                Console.WriteLine($"❌ Error syncing recommendations for user {userId}: {ex.Message}");
                throw;
            }
        }
    }
}