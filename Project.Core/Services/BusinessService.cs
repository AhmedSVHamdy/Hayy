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
        private readonly IImageService _imageService;
        private readonly IAdminService _adminService;
        private readonly IMapper _mapper; // 👈 ضروري جداً للـ Update

        public BusinessService(
            IBusinessRepository businessRepo,
            IImageService imageService,
            IAdminService adminService,
            IMapper mapper)
        {
            _businessRepo = businessRepo;
            _imageService = imageService;
            _adminService = adminService;
            _mapper = mapper;
        }

        // =========================================================
        //  1. تقديم طلب توثيق جديد أو إعادة تقديم (Onboarding)
        // =========================================================
        public async Task SubmitBusinessDetailsAsync(Guid userId, BusinessOnboardingDTO model)
        {
            // أ. التحقق من وجود بيزنس مسبق
            var existingBusiness = await _businessRepo.GetBusinessByUserIdAsync(userId);

            if (existingBusiness != null)
            {
                // 🛑 لو البيزنس موجود وحالته Pending أو Verified -> نمنعه
                if (existingBusiness.VerificationStatus != VerificationStatus.Rejected)
                {
                    throw new InvalidOperationException("You already have a business registered correctly.");
                }

                // ✅ لو البيزنس موجود وحالته Rejected -> نسمح له بالتعديل (Re-submission Flow)

                // 1. رفع الصور الجديدة (لأن القديمة كانت سبب الرفض غالباً)
                var logoTaskResub = _imageService.UploadImageAsync(model.LogoImage);
                var regTaskResub = _imageService.UploadImageAsync(model.CommercialRegImage);
                var taxTaskResub = _imageService.UploadImageAsync(model.TaxCardImage);
                var identityTaskResub = _imageService.UploadImageAsync(model.IdentityCardImage);

                await Task.WhenAll(logoTaskResub, regTaskResub, taxTaskResub, identityTaskResub);

                // 2. تحديث بيانات البيزنس الأساسية (الاسم، اللوجو، الخ)
                _mapper.Map(model, existingBusiness);
                existingBusiness.LogoImage = await logoTaskResub;
                existingBusiness.VerificationStatus = VerificationStatus.Pending; // نرجعه قيد المراجعة

                // 3. إنشاء طلب توثيق جديد في الهيستوري
                var newVerification = new BusinessVerification
                {
                    Id = Guid.NewGuid(),
                    BusinessId = existingBusiness.Id,
                    CommercialRegImage = await regTaskResub,
                    TaxCardImage = await taxTaskResub,
                    IdentityCardImage = await identityTaskResub,
                    Status = VerificationStatus.Pending,
                    SubmittedAt = DateTime.UtcNow,
                    AdminId = null
                };

                // 4. الحفظ
                await _businessRepo.AddVerificationAsync(newVerification);
                await _businessRepo.UpdateBusinessAsync(existingBusiness);

                return; // خروج (تمت عملية التعديل بنجاح)
            }

            // =========================================================
            //  لو مستخدم جديد (New Business Flow)
            // =========================================================

            // ب. التحقق من وجود الملفات
            if (model.LogoImage == null || model.CommercialRegImage == null ||
                model.TaxCardImage == null || model.IdentityCardImage == null)
            {
                throw new ArgumentException("All images are required for new registration.");
            }

            // ج. رفع الصور بشكل متوازي 🚀
            var logoTask = _imageService.UploadImageAsync(model.LogoImage);
            var regTask = _imageService.UploadImageAsync(model.CommercialRegImage);
            var taxTask = _imageService.UploadImageAsync(model.TaxCardImage);
            var identityTask = _imageService.UploadImageAsync(model.IdentityCardImage);

            await Task.WhenAll(logoTask, regTask, taxTask, identityTask);

            // د. تجهيز البيانات
            var businessId = Guid.NewGuid();

            // 1. إنشاء كيان الشركة
            var business = _mapper.Map<Business>(model); // نستخدم المابر هنا للسهولة
            business.Id = businessId;
            business.UserId = userId;
            business.LogoImage = await logoTask;
            business.VerificationStatus = VerificationStatus.Pending;
            business.CreatedAt = DateTime.UtcNow;

            // 2. إنشاء كيان التوثيق (أول سجل)
            var verification = new BusinessVerification
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                CommercialRegImage = await regTask,
                TaxCardImage = await taxTask,
                IdentityCardImage = await identityTask,
                Status = VerificationStatus.Pending,
                SubmittedAt = DateTime.UtcNow,
                AdminId = null
            };

            // هـ. الحفظ في قاعدة البيانات
            // (يفضل استخدام Transaction هنا لو أمكن، لكن الكود المتتابع مقبول)
            await _businessRepo.AddBusinessAsync(business);
            await _businessRepo.AddVerificationAsync(verification);
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

                // بنجيب تاريخ آخر طلب تقديم
                SubmittedAt = b.Verifications != null && b.Verifications.Any()
                    ? b.Verifications.OrderByDescending(v => v.SubmittedAt).First().SubmittedAt
                    : b.CreatedAt,

                // لو عاوز تعرض صورة السجل التجاري في القائمة عشان الأدمن يشوفها بسرعة
                CommercialRegImage = b.Verifications != null && b.Verifications.Any()
                    ? b.Verifications.OrderByDescending(v => v.SubmittedAt).First().CommercialRegImage
                    : null
            }).ToList();
        }

        // =========================================================
        //  3. مراجعة الطلب (Approve / Reject)
        // =========================================================
        public async Task ReviewBusinessAsync(Guid businessId, ReviewBusinessDTO reviewDto, Guid adminId)
        {
            // أ. جلب الشركة
            var business = await _businessRepo.GetBusinessByIdAsync(businessId);
            if (business == null) throw new KeyNotFoundException("Business not found.");

            // ب. جلب أحدث طلب توثيق (الذي حالته Pending)
            // ملاحظة: بنجيب آخر واحد عشان نعدل عليه
            var verification = business.Verifications
                                .OrderByDescending(v => v.SubmittedAt)
                                .FirstOrDefault();

            if (verification == null) throw new KeyNotFoundException("No verification request found.");

            // ج. تحديد الحالة الجديدة
            var newStatus = reviewDto.IsApproved ? VerificationStatus.Verified : VerificationStatus.Rejected;

            // د. تحديث بيانات التوثيق (السجل)
            verification.Status = newStatus;
            verification.ReviewedAt = DateTime.UtcNow;
            verification.AdminId = adminId;
            verification.RejectionReason = reviewDto.IsApproved ? null : reviewDto.Reason;

            // هـ. تحديث حالة البيزنس الكلية
            business.VerificationStatus = newStatus;

            // و. الحفظ
            await _businessRepo.UpdateVerificationAsync(verification);
            await _businessRepo.UpdateBusinessAsync(business);

            // ز. تسجيل في Audit Log
            await _adminService.LogAdminActionAsync(
                adminId: adminId,
                actionType: reviewDto.IsApproved ? AdminActionType.Verified : AdminActionType.Rejected,
                targetType: TargetType.Business,
                targetId: businessId.ToString(),
                notes: reviewDto.IsApproved ? "Business Approved" : $"Rejected: {reviewDto.Reason}"
            );
        }
    }
}