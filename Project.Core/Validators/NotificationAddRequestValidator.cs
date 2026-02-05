using FluentValidation;
using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Validators
{
    public class NotificationAddRequestValidator : AbstractValidator<NotificationAddRequest>
    {
        public NotificationAddRequestValidator()
        {
            // 1. التحقق من العنوان (Title)
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("عنوان الإشعار مطلوب") // مينفعش فاضي
                .NotNull()
                .MaximumLength(100).WithMessage("عنوان الإشعار لا يجب أن يتجاوز 100 حرف");

            // 2. التحقق من المحتوى (Body)
            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("محتوى الإشعار مطلوب")
                .MaximumLength(500).WithMessage("محتوى الإشعار طويل جداً");

            // 3. التحقق من الـ UserId (أهم حاجة عشان نعرف هنبعت لمين)
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("يجب تحديد المستخدم المرسل إليه")
                .NotEqual(Guid.Empty).WithMessage("رقم المستخدم غير صحيح");

            // 4. التحقق من النوع (Type)
            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("نوع الإشعار مطلوب");

            
        }
    }
}
