using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CerateBusinessPostDto;

namespace Project.Core.Validators
{
    public class CreateBusinessPostValidator : AbstractValidator<CreatePostDto>
    {
        public CreateBusinessPostValidator()
        {
            // 1. التأكد من إن الـ PlaceId مش فاضي
            RuleFor(x => x.PlaceId)
                .NotEmpty().WithMessage("لازم تختار المطعم أو المكان! 🏪");

            // 2. التحقق من طول المحتوى (مثلاً 2000 حرف كحد أقصى)
            RuleFor(x => x.Content)
                .MaximumLength(2000).WithMessage("البوست طويل جداً! خف شوية 😅");

            // 3. ⚠️ القاعدة الذهبية: يا إما في كلام، يا إما في صورة (أو الاتنين)
            // التعديل هنا: استخدمنا ImageFile بدل PostAttachments
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.Content) || (x.ImageFile != null && x.ImageFile.Length > 0))
                .WithMessage("البوست لازم يكون فيه محتوى أو صورة على الأقل! 📝📸");
        }
    }
}