using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateEventBooking;

namespace Project.Core.Validators
{
    public class CreateBookingValidator : AbstractValidator<CreateBookingDto>
    {
        public CreateBookingValidator()
        {
            RuleFor(x => x.EventId)
                .NotEmpty().WithMessage("يجب تحديد الحدث المراد حجزه.");

            RuleFor(x => x.TicketQuantity)
                .GreaterThan(0).WithMessage("عدد التذاكر يجب أن يكون أكبر من صفر.")
                .LessThanOrEqualTo(5).WithMessage("الحد الأقصى للحجز هو 5 تذاكر في المرة الواحدة."); // حماية من الاحتكار
        }
    }
}
