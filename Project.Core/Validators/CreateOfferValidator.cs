using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateOfferDTO;

namespace Project.Core.Validators
{
    public class CreateOfferValidator : AbstractValidator<CreateOfferDto>
    {
        public CreateOfferValidator()
        {
            RuleFor(x => x.PlaceId).NotEmpty().WithMessage("معرف المكان مطلوب.");
            RuleFor(x => x.Title).NotEmpty().WithMessage("عنوان العرض مطلوب.");
            RuleFor(x => x.Discount)
                .GreaterThan(0).WithMessage("نسبة الخصم يجب أن تكون أكبر من صفر.")
                .LessThanOrEqualTo(100).WithMessage("نسبة الخصم لا يمكن أن تتخطى 100%.");

            RuleFor(x => x.StartDate)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("تاريخ البدء لا يمكن أن يكون في الماضي.");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate).WithMessage("تاريخ الانتهاء يجب أن يكون بعد تاريخ البدء.");
        }
    }
    public class UpdateOfferValidator : AbstractValidator<UpdateOfferDto>
    {
        public UpdateOfferValidator()
        {
            // 🔥 التأكد إن الـ ID مبعوت عشان نعرف هنعدل إيه
            RuleFor(x => x.Id).NotEmpty().WithMessage("معرف العرض مطلوب.");

            RuleFor(x => x.Title).NotEmpty().WithMessage("عنوان العرض مطلوب.");

            RuleFor(x => x.Discount)
                .GreaterThan(0).WithMessage("نسبة الخصم يجب أن تكون أكبر من صفر.")
                .LessThanOrEqualTo(100).WithMessage("نسبة الخصم لا يمكن أن تتخطى 100%.");

            // 💡 ملاحظة: مش هنشترط هنا إن تاريخ البدء في المستقبل، عشان العرض ممكن يكون شغال بالفعل وبيتم تعديله

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate).WithMessage("تاريخ الانتهاء يجب أن يكون بعد تاريخ البدء.");

            // التأكد إن الحالة (Status) المبعوتة موجودة فعلاً في الـ Enum
            RuleFor(x => x.Status).IsInEnum().WithMessage("حالة العرض غير صحيحة.");
        }
    }
}
