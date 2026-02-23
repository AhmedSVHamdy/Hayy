using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateEventDTO;

namespace Project.Core.Validators
{
    public class EventCreateValidator : AbstractValidator<EventCreateDto>
    {
        public EventCreateValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("عنوان الحدث مطلوب").MaximumLength(200);
            RuleFor(x => x.PlaceId).NotEmpty().WithMessage("يجب تحديد المكان");
            RuleFor(x => x.Datetime).GreaterThan(DateTime.Now).WithMessage("تاريخ الحدث يجب أن يكون في المستقبل");
            RuleFor(x => x.Capacity).GreaterThan(0).WithMessage("سعة الحدث يجب أن تكون أكبر من صفر");
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("السعر لا يمكن أن يكون بالسالب");

            // تحقق منطقي: لو فعلنا قائمة الانتظار، لازم نحدد حد أقصى لها
            RuleFor(x => x.WaitlistLimit)
                .GreaterThan(0).When(x => x.IsWaitlistEnabled)
                .WithMessage("يجب تحديد الحد الأقصى لقائمة الانتظار");
        }
    }
}
