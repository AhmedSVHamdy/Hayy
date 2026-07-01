using AutoMapper;
using FluentValidation;
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
        private readonly IValidator<BusinessOnboardingDTO> _businessValidator; // ✅ تم تعديل الـ Type هنا
        private readonly IMapper _mapper;

        public BusinessService(
            IBusinessRepository businessRepo,
            IImageService imageService,
            IAdminService adminService,
            IValidator<BusinessOnboardingDTO> businessValidator, // ✅ تم التعديل هنا أيضاً
            IMapper mapper)
        {
            _businessRepo = businessRepo;
            _imageService = imageService;
            _adminService = adminService;
            _businessValidator = businessValidator;
            _mapper = mapper;
        }

        // =========================================================
        //  1. تقديم طلب توثيق جديد أو إعادة تقديم (Onboarding)
        // =========================================================
        public async Task SubmitBusinessDetailsAsync(Guid userId, BusinessOnboardingDTO model)
        {
            // ✅ تشغيل الفاليديشن والتحقق من النتيجة
            var valResult = await _businessValidator.ValidateAsync(model);
            if (!valResult.IsValid)
            {
                throw new ValidationException(valResult.Errors);
            }

            var existingBusiness = await _businessRepo.GetBusinessByUserIdAsync(userId);

            if (existingBusiness != null)
            {
                if (existingBusiness.VerificationStatus != VerificationStatus.Rejected)
                    throw new InvalidOperationException("You already have a business registered correctly.");

                // ✅ Re-submission Flow
                var logoTask = _imageService.UploadImageAsync(model.LogoImage);
                var regTask = _imageService.UploadImageAsync(model.CommercialRegImage);
                var taxTask = _imageService.UploadImageAsync(model.TaxCardImage);
                var identityTask = _imageService.UploadImageAsync(model.IdentityCardImage);

                await Task.WhenAll(logoTask, regTask, taxTask, identityTask);

                _mapper.Map(model, existingBusiness);
                existingBusiness.LogoImage = await logoTask; // ✅ استخدام await المباشر أفضل وأنظف من .Result
                existingBusiness.VerificationStatus = VerificationStatus.Pending;

                var newVerification = new BusinessVerification
                {
                    Id = Guid.NewGuid(),
                    BusinessId = existingBusiness.Id,
                    CommercialRegImage = await regTask,
                    TaxCardImage = await taxTask,
                    IdentityCardImage = await identityTask,
                    Status = VerificationStatus.Pending,
                    SubmittedAt = DateTime.UtcNow,
                    AdminId = null
                };

                await _businessRepo.AddVerificationAsync(newVerification);
                await _businessRepo.UpdateBusinessAsync(existingBusiness);
                return;
            }

            // =========================================================
            //  New Business Flow
            // =========================================================
            // ملحوظة: هذا التحقق أصبح إضافياً لأن الـ Validator يقوم به بالفعل، لكنه ممتاز كـ Defensive Programming
            if (model.LogoImage == null || model.CommercialRegImage == null ||
                model.TaxCardImage == null || model.IdentityCardImage == null)
            {
                throw new ArgumentException("All images are required for new registration.");
            }

            var newLogoTask = _imageService.UploadImageAsync(model.LogoImage);
            var newRegTask = _imageService.UploadImageAsync(model.CommercialRegImage);
            var newTaxTask = _imageService.UploadImageAsync(model.TaxCardImage);
            var newIdentityTask = _imageService.UploadImageAsync(model.IdentityCardImage);

            await Task.WhenAll(newLogoTask, newRegTask, newTaxTask, newIdentityTask);

            var businessId = Guid.NewGuid();

            var business = _mapper.Map<Business>(model);
            business.Id = businessId;
            business.UserId = userId;
            business.LogoImage = await newLogoTask;
            business.VerificationStatus = VerificationStatus.Pending;
            business.CreatedAt = DateTime.UtcNow;

            var verification = new BusinessVerification
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                CommercialRegImage = await newRegTask,
                TaxCardImage = await newTaxTask,
                IdentityCardImage = await newIdentityTask,
                Status = VerificationStatus.Pending,
                SubmittedAt = DateTime.UtcNow,
                AdminId = null
            };

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

                SubmittedAt = b.Verifications != null && b.Verifications.Any()
                    ? b.Verifications.OrderByDescending(v => v.SubmittedAt).First().SubmittedAt
                    : b.CreatedAt,

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
            var business = await _businessRepo.GetBusinessByIdAsync(businessId);
            if (business == null) throw new KeyNotFoundException("Business not found.");

            var verification = business.Verifications
                                .OrderByDescending(v => v.SubmittedAt)
                                .FirstOrDefault();

            if (verification == null) throw new KeyNotFoundException("No verification request found.");

            var newStatus = reviewDto.IsApproved ? VerificationStatus.Verified : VerificationStatus.Rejected;

            verification.Status = newStatus;
            verification.ReviewedAt = DateTime.UtcNow;
            verification.AdminId = adminId;
            verification.RejectionReason = reviewDto.IsApproved ? null : reviewDto.Reason;

            business.VerificationStatus = newStatus;

            await _businessRepo.UpdateVerificationAsync(verification);
            await _businessRepo.UpdateBusinessAsync(business);

            await _adminService.LogAdminActionAsync(
                adminId: adminId,
                actionType: reviewDto.IsApproved ? AdminActionType.Verified : AdminActionType.Rejected,
                targetType: TargetType.Business,
                targetId: businessId.ToString(),
                notes: reviewDto.IsApproved ? "Business Approved" : $"Rejected: {reviewDto.Reason}"
            );
        }

        // =========================================================
        //  4. جلب بروفايل البيزنس للمستخدم الحالي
        // =========================================================
        public async Task<BusinessProfileDTO> GetBusinessProfileByUserIdAsync(Guid userId)
        {
            var business = await _businessRepo.GetBusinessByUserIdAsync(userId);

            if (business == null)
                throw new KeyNotFoundException("No business registered for this user.");

            // جلب آخر طلب توثيق لمعرفة سبب الرفض إن وجد
            var latestVerification = business.Verifications?
                .OrderByDescending(v => v.SubmittedAt)
                .FirstOrDefault();

            var profileDto = _mapper.Map<BusinessProfileDTO>(business);
            profileDto.VerificationStatus = business.VerificationStatus.ToString();
            profileDto.RejectionReason = latestVerification?.RejectionReason;

            return profileDto;
        }

        // =========================================================
        //  5. تعديل بيانات البروفايل (الاسم، اللوجو)
        // =========================================================
        public async Task UpdateBusinessProfileAsync(Guid userId, UpdateBusinessProfileDTO model)
        {
            var business = await _businessRepo.GetBusinessByUserIdAsync(userId);

            if (business == null)
                throw new KeyNotFoundException("Business not found.");

            // تحديث البيانات النصية
            business.BrandName = model.BrandName;
            business.LegalName = model.LegalName;

            // لو رفع لوجو جديد، نقوم برفعه وتحديث الرابط
            if (model.NewLogoImage != null && model.NewLogoImage.Length > 0)
            {
                // ملحوظة: يفضل عمل فالييدشن للوجو الجديد هنا أو عبر Validator مستقل
                var logoUrl = await _imageService.UploadImageAsync(model.NewLogoImage);
                business.LogoImage = logoUrl;
            }

            await _businessRepo.UpdateBusinessAsync(business);
        }



        // =========================================================
        //  7. جلب إيميل صاحب العمل عن طريق الـ Business ID
        // =========================================================
        public async Task<BusinessUserEmailDTO> GetBusinessUserEmailAsync(Guid businessId)
        {
            // جلب بيانات البيزنس من الـ Repo
            var business = await _businessRepo.GetBusinessByIdAsync(businessId);

            if (business == null)
                throw new KeyNotFoundException("Business not found.");

            // التحقق من أن البيزنس مربوط بمستخدم وأن الإيميل متوفر
            if (business.User == null || string.IsNullOrEmpty(business.User.Email))
            {
                throw new InvalidOperationException("User account associated with this business is missing or has no email.");
            }

            return new BusinessUserEmailDTO
            {
                BusinessId = business.Id,
                Email = business.User.Email
            };
        }




        // =========================================================
        // 1. دالة مزامنة وتخزين البيانات الحالية (تُشغل مرة واحدة للتهيئة)
        // =========================================================
        public async Task SyncAllBusinessesAnalyticsAsync()
        {
            // جلب كل الشركات بكل تفاصيلها الحالية
            var allBusinesses = await _businessRepo.GetAllBusinessesWithDetailsForSyncAsync();

            foreach (var business in allBusinesses)
            {
                // حساب القيم الحالية من الجداول الأخرى
                int totalFollowers = business.Places.Sum(p => p.PlaceFollows.Count);
                int totalReviews = business.Places.Sum(p => p.Reviews.Count);

                decimal avgRating = 0;
                var allReviews = business.Places.SelectMany(p => p.Reviews).ToList();
                if (allReviews.Any())
                {
                    avgRating = (decimal)allReviews.Average(r => r.Rating);
                }

                decimal monthlyRevenue = business.Subscriptions != null
                    ? business.Subscriptions
                        .Where(s => s.IsActive && s.StartDate <= DateTime.UtcNow && s.EndDate >= DateTime.UtcNow)
                        .Sum(s => s.Plan?.Price ?? 0)
                    : 0;

                // ✅ التعديل هنا: تحديد ما إذا كان السجل جديداً أم موجوداً مسبقاً
                bool isNew = false;
                var analytics = business.BusinessAnalytics;

                if (analytics == null)
                {
                    analytics = new BusinessAnalytic
                    {
                        Id = Guid.NewGuid(),
                        BusinessId = business.Id
                    };
                    isNew = true; // نبلغ النظام أن هذا السجل جديد ويحتاج Insert
                }

                // تخزين القيم
                analytics.TotalViews = 0;
                analytics.TotalFollowers = totalFollowers;
                analytics.TotalReviews = totalReviews;
                analytics.AvgRating = Math.Round(avgRating, 2);
                analytics.MonthlyRevenue = monthlyRevenue;
                analytics.LastUpdated = DateTime.UtcNow;

                // ✅ التوجيه الصحيح لـ EF Core لتجنب الخطأ
                if (isNew)
                {
                    await _businessRepo.AddBusinessAnalyticAsync(analytics);
                }
            }

            // ✅ حفظ كل الشركات بضربة واحدة في الداتابيز (أداء ممتاز)
            await _businessRepo.SaveChangesAsync();
        }




        // =========================================================
        // 2. دالة الـ GET الجديدة (طائرة وسريعة جداً) 🚀
        // =========================================================
        public async Task<BusinessAnalyticDTO> GetMyAnalyticsAsync(Guid userId)
        {
            var business = await _businessRepo.GetBusinessByUserIdAsync(userId);
            if (business == null)
                throw new KeyNotFoundException("No business registered for this user.");

            // لو ملوش سجل (رغم المزامنة)، نرجع كائن فارغ كحماية من الـ Crash
            if (business.BusinessAnalytics == null)
            {
                return new BusinessAnalyticDTO { LastUpdated = DateTime.UtcNow };
            }

            // قراءة البيانات المحفوظة مسبقاً فوراً بدون أي Includes ثقيلة أو حسابات!
            return _mapper.Map<BusinessAnalyticDTO>(business.BusinessAnalytics);
        }
    }
}