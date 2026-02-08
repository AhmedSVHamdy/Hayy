using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Core.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly IBusinessRepository _businessRepo;
        private readonly INotificationService _notificationService; // (لو مجهزها)
        private readonly IImageService _imageService;
        private readonly IAdminService _adminService; // خدمة الأدمن للوج

        public BusinessService(
            IBusinessRepository businessRepo,
            INotificationService notificationService,
            IImageService imageService,
            IAdminService adminService)
        {
            _businessRepo = businessRepo;
            _notificationService = notificationService;
            _imageService = imageService;
            _adminService = adminService;
        }

        // =========================================================
        //  1. تقديم طلب توثيق جديد (Onboarding)
        // =========================================================
        public async Task SubmitBusinessDetailsAsync(Guid userId, BusinessOnboardingDTO model)
        {
            // أ. التحقق من عدم وجود بيزنس مسبق لنفس المستخدم
            var existingBusiness = await _businessRepo.GetByUserIdAsync(userId);
            if (existingBusiness != null)
            {
                throw new InvalidOperationException("User already has a business profile.");
            }

            // ب. التحقق من وجود الملفات
            if (model.LogoImage == null || model.CommercialRegImage == null ||
                model.TaxCardImage == null || model.IdentityCardImage == null)
            {
                throw new ArgumentException("All images are required.");
            }

            // ج. رفع الصور بشكل متوازي (Performance Optimization) 🚀
            var logoTask = _imageService.UploadImageAsync(model.LogoImage);
            var regTask = _imageService.UploadImageAsync(model.CommercialRegImage);
            var taxTask = _imageService.UploadImageAsync(model.TaxCardImage);
            var identityTask = _imageService.UploadImageAsync(model.IdentityCardImage);

            await Task.WhenAll(logoTask, regTask, taxTask, identityTask);

            // د. تجهيز البيانات
            var businessId = Guid.NewGuid(); // إنشاء ID موحد

            // 1. إنشاء كيان الشركة
            var business = new Business
            {
                Id = businessId,
                UserId = userId,
                BrandName = model.BrandName,
                LegalName = model.LegalName,
                CommercialRegNumber = model.CommercialRegNumber,
                TaxNumber = model.TaxNumber,
                LogoImage = await logoTask,
                VerificationStatus = VerificationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // 2. إنشاء كيان التوثيق (أول سجل في الهيستوري)
            var verification = new BusinessVerification
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                CommercialRegImage = await regTask,
                TaxCardImage = await taxTask,
                IdentityCardImage = await identityTask, // تصحيح: المتغير هنا identityTask نتيجته
                Status = VerificationStatus.Pending,
                SubmittedAt = DateTime.UtcNow,
                AdminId = null // لم يراجعه أحد بعد
            };

            // تصحيح بسيط للسطر السابق (Identity path):
            verification.IdentityCardImage = await identityTask;

            // هـ. الحفظ في قاعدة البيانات
            try
            {
                // نحفظ البيزنس الأول عشان الـ Foreign Key
                await _businessRepo.AddAsync(business);
                // وبعدين نحفظ طلب التوثيق
                await _businessRepo.AddVerificationAsync(verification);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving business: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // =========================================================
        //  2. جلب الطلبات المعلقة (Admin Dashboard)
        // =========================================================
        public async Task<List<BusinessVerificationSummaryDTO>> GetPendingVerificationsAsync()
        {
            var pendingBusinesses = await _businessRepo.GetPendingVerificationsAsync();

            return pendingBusinesses.Select(b => new BusinessVerificationSummaryDTO
            {
                BusinessId = b.Id,
                BrandName = b.BrandName,
                CommercialRegNumber = b.CommercialRegNumber,
                Status = b.VerificationStatus.ToString(),

                // 👇 بنجيب تاريخ أحدث محاولة توثيق
                SubmittedAt = b.Verifications
                               .OrderByDescending(v => v.SubmittedAt)
                               .FirstOrDefault()?.SubmittedAt ?? b.CreatedAt
            }).ToList();
        }

        // =========================================================
        //  3. مراجعة الطلب (Approve / Reject)
        // =========================================================
        public async Task ReviewBusinessAsync(Guid businessId, ReviewBusinessDTO reviewDto, Guid adminId)
        {
            // أ. جلب الشركة
            var business = await _businessRepo.GetByIdAsync(businessId);
            if (business == null) throw new KeyNotFoundException("Business not found.");

            // ب. جلب أحدث طلب توثيق
            var verification = await _businessRepo.GetLatestVerificationByBusinessIdAsync(businessId);
            if (verification == null) throw new KeyNotFoundException("Verification request not found.");

            // ج. تحديد الحالة الجديدة
            var newStatus = reviewDto.IsApproved ? VerificationStatus.Verified : VerificationStatus.Rejected;

            // د. تحديث البيانات
            verification.Status = newStatus;
            verification.ReviewedAt = DateTime.UtcNow;
            verification.AdminId = adminId;
            verification.RejectionReason = reviewDto.IsApproved ? null : reviewDto.Reason;

            business.VerificationStatus = newStatus;

            // هـ. الحفظ
            await _businessRepo.UpdateVerificationAsync(verification);
            await _businessRepo.UpdateAsync(business);

            // و. 🔥 تسجيل في سجل الأدمن (Audit Log)
            await _adminService.LogAdminActionAsync(
                adminId: adminId,
                actionType: reviewDto.IsApproved ? AdminActionType.Verified : AdminActionType.Rejected,
                targetType: TargetType.Business,
                targetId: businessId.ToString(),
                notes: reviewDto.Reason
            );

            // ز. إرسال إشعار (اختياري)
            // await _notificationService.SendAsync(business.UserId, "Your verification status has been updated.");
        }
    }
}