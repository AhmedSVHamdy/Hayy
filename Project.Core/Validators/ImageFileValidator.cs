using FluentValidation;

namespace Project.Core.Validators
{
    // Validator خاص للملفات عشان منعيدش الكود
    public class ImageFileValidator : AbstractValidator<Microsoft.AspNetCore.Http.IFormFile>
    {
        public ImageFileValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("File is required")
                .Must(x => x.Length > 0).WithMessage("File cannot be empty")
                .Must(x => x.Length <= 5 * 1024 * 1024).WithMessage("File size must be less than 5MB")
                .Must(x => x.ContentType.Equals("image/jpeg") || x.ContentType.Equals("image/png") || x.ContentType.Equals("image/jpg"))
                .WithMessage("File must be an image (jpg, jpeg, png)");
        }
    }






}
    

