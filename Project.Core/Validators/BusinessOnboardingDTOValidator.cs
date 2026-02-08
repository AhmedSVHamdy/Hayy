using FluentValidation;
using Microsoft.AspNetCore.Http;
using Project.Core.DTO;

namespace Project.Core.Validators
{
    public partial class ChangePasswordValidator
    {
        public class BusinessOnboardingDTOValidator : AbstractValidator<BusinessOnboardingDTO>
        {
            public BusinessOnboardingDTOValidator()
            {
                // 1. Brand Name
                RuleFor(x => x.BrandName)
                    .NotEmpty().WithMessage("Brand Name is required.")
                    .MaximumLength(100).WithMessage("Brand Name cannot exceed 100 characters.");

                // 2. Legal Name
                RuleFor(x => x.LegalName)
                    .NotEmpty().WithMessage("Legal Name is required.")
                    .MaximumLength(150).WithMessage("Legal Name cannot exceed 150 characters.");

                // 3. Commercial Registration Number
                RuleFor(x => x.CommercialRegNumber)
                    .NotEmpty().WithMessage("Commercial Registration Number is required.")
                    .Matches(@"^\d+$").WithMessage("Commercial Registration Number must contain digits only.")
                    .Length(5, 20).WithMessage("Commercial Registration Number must be between 5 and 20 digits.");

                // 4. Tax Number
                RuleFor(x => x.TaxNumber)
                    .NotEmpty().WithMessage("Tax Number is required.")
                    .Matches(@"^\d+$").WithMessage("Tax Number must contain digits only.")
                    .Length(9, 15).WithMessage("Tax Number must be between 9 and 15 digits.");

                // =================================================
                // 5. File Validations
                // =================================================

                // Logo
                RuleFor(x => x.LogoImage)
                    .NotNull().WithMessage("Logo image is required.")
                    .Must(BeAValidImage).WithMessage("Invalid file format. Only JPG, JPEG, and PNG are allowed.")
                    .Must(HaveValidSize).WithMessage("File size must not exceed 2 MB.");

                // Commercial Registry Image
                RuleFor(x => x.CommercialRegImage)
                    .NotNull().WithMessage("Commercial Registry document is required.")
                    .Must(BeAValidImage).WithMessage("Invalid file format. Only JPG, JPEG, and PNG are allowed.")
                    .Must(HaveValidSize).WithMessage("File size must not exceed 2 MB.");

                // Tax Card Image
                RuleFor(x => x.TaxCardImage)
                    .NotNull().WithMessage("Tax Card document is required.")
                    .Must(BeAValidImage).WithMessage("Invalid file format. Only JPG, JPEG, and PNG are allowed.")
                    .Must(HaveValidSize).WithMessage("File size must not exceed 2 MB.");

                // Identity Card Image
                RuleFor(x => x.IdentityCardImage)
                    .NotNull().WithMessage("Identity Card document is required.")
                    .Must(BeAValidImage).WithMessage("Invalid file format. Only JPG, JPEG, and PNG are allowed.")
                    .Must(HaveValidSize).WithMessage("File size must not exceed 2 MB.");
            }

            // --- Helper Functions ---
            private bool BeAValidImage(IFormFile? file)
            {
                if (file == null) return true;
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                return allowedExtensions.Contains(extension);
            }

            private bool HaveValidSize(IFormFile? file)
            {
                if (file == null) return true;
                return file.Length <= 2 * 1024 * 1024; // 2 MB
            }
        }
    }
}
