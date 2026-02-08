using FluentValidation;
using Project.Core.DTO;

namespace Project.Core.Validators
{
    public partial class ChangePasswordValidator
    {
        public class ReviewBusinessValidator : AbstractValidator<ReviewBusinessDTO>
        {

            public ReviewBusinessValidator()
            {
                // IsApproved مطلوب (Boolean دائماً له قيمة، لكن للتوضيح)
                RuleFor(x => x.IsApproved)
                    .NotNull();

                // قاعدة شرطية: السبب مطلوب فقط في حالة الرفض
                RuleFor(x => x.Reason)
                    .NotEmpty()
                    .When(x => x.IsApproved == false)
                    .WithMessage("Reason is required when rejecting a business request.");

                // في حالة الموافقة، يمكن أن يكون السبب فارغاً، لكن لو كتب شيئاً لا يتعدى 1000 حرف
                RuleFor(x => x.Reason)
                    .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters.");
            }
        }
    }
}

