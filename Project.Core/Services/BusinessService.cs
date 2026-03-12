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
        private readonly IMapper _mapper;

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
            var existingBusiness = await _businessRepo.GetBusinessByUserIdAsync(userId);

            if (existingBusiness != null)
            {
                if (existingBusiness.VerificationStatus != VerificationStatus.Rejected)
                    throw new InvalidOperationException("You already have a business registered correctly.");

                // ✅ Re-submission Flow
                var logoTaskResub = _imageService.UploadImageAsync(model.LogoImage);
                var regTaskResub = _imageService.UploadImageAsync(model.CommercialRegImage);
                var taxTaskResub = _imageService.UploadImageAsync(model.TaxCardImage);
                var identityTaskResub = _imageService.UploadImageAsync(model.IdentityCardImage);

                await Task.WhenAll(logoTaskResub, regTaskResub, taxTaskResub, identityTaskResub);

                _mapper.Map(model, existingBusiness);
                existingBusiness.LogoImage = logoTaskResub.Result;      // ✅ Result بدل await
                existingBusiness.VerificationStatus = VerificationStatus.Pending;

                var newVerification = new BusinessVerification
                {
                    Id = Guid.NewGuid(),
                    BusinessId = existingBusiness.Id,
                    CommercialRegImage = regTaskResub.Result,                      // ✅ Result بدل await
                    TaxCardImage = taxTaskResub.Result,                      // ✅ Result بدل await
                    IdentityCardImage = identityTaskResub.Result,                 // ✅ Result بدل await
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

            if (model.LogoImage == null || model.CommercialRegImage == null ||
                model.TaxCardImage == null || model.IdentityCardImage == null)
            {
                throw new ArgumentException("All images are required for new registration.");
            }

            var logoTask = _imageService.UploadImageAsync(model.LogoImage);
            var regTask = _imageService.UploadImageAsync(model.CommercialRegImage);
            var taxTask = _imageService.UploadImageAsync(model.TaxCardImage);
            var identityTask = _imageService.UploadImageAsync(model.IdentityCardImage);

            await Task.WhenAll(logoTask, regTask, taxTask, identityTask);

            var businessId = Guid.NewGuid();

            var business = _mapper.Map<Business>(model);
            business.Id = businessId;
            business.UserId = userId;
            business.LogoImage = logoTask.Result;                         // ✅ Result بدل await
            business.VerificationStatus = VerificationStatus.Pending;
            business.CreatedAt = DateTime.UtcNow;

            var verification = new BusinessVerification
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                CommercialRegImage = regTask.Result,                               // ✅ Result بدل await
                TaxCardImage = taxTask.Result,                               // ✅ Result بدل await
                IdentityCardImage = identityTask.Result,                          // ✅ Result بدل await
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
    }
}