using AutoMapper;
using Project.Core.Domain.Entities.NotificationPayload;
using Project.Core.Domain.RopositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly IBusinessRepository _businessRepo;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper; // 👈 المابر

        public BusinessService(
            IBusinessRepository businessRepo,
            INotificationService notificationService,
            IMapper mapper) // بنحقنه هنا
        {
            _businessRepo = businessRepo;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task<BusinessResponse?> ApproveBusinessProfile(Guid businessId)
        {
            if (businessId == Guid.Empty)
            {
                throw new ArgumentException("رقم المعرف (ID) غير صحيح.");
                // الميدلوير هيرجع 400 Bad Request
            }

            // 1. هات البيزنس
            var business = await _businessRepo.GetByIdAsync(businessId);

            if (business == null)
            {
                // بدل ما ترجع null، ارمي اكسبشن عشان الميدلوير يرجع 404
                throw new KeyNotFoundException($"لم يتم العثور على نشاط تجاري بالمعرف: {businessId}");
            }

            // لو هو أصلاً متوافق عليه، منكملش عشان منبعتش إشعار تاني
            if (business.VerificationStatus == VerificationStatus.Verified)
            {
                throw new InvalidOperationException("هذا النشاط التجاري موثق بالفعل!");
                // الميدلوير ممكن يهندل دي كـ 400 أو 409 Conflict
            }

            // ---------------------------------------------------------
            // ✅ طالما عدينا الحراس اللي فوق، كمل الشغل بأمان
            // ---------------------------------------------------------

            // 2. تحديث الحالة
            business.VerificationStatus = VerificationStatus.Verified;
            // business.VerifiedAt = DateTime.UtcNow; 

            await _businessRepo.UpdateAsync(business);

            // 3. إرسال الإشعار
            var notification = new NotificationAddRequest
            {
                UserId = business.UserId,
                Title = $"تم توثيق {business.BrandName}! ✅",
                Message = "حسابك التجاري الآن نشط وجاهز للعمل.",
                Type = "BusinessVerification",
                ReferenceId = business.Id.ToString(),
                // داتا إضافية عشان شكل الإشعار
                Data = new NotificationData
                {
                    UserName = "Administration",
                    ItemCount = 1
                }
            };

            // الـ NotificationService جواها الفاليديشن الخاص بيها، فمش محتاج تقلق هنا
            await _notificationService.CreateNotification(notification);

            // 4. التحويل والإرجاع
            var responseDto = _mapper.Map<BusinessResponse>(business);
            return responseDto;
        }
    }
}
