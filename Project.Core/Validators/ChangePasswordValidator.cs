using FluentValidation;
using Project.Core.DTO;

namespace Project.Core.Validators
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
                // يمكنك إضافة شروط إضافية هنا (حروف كبيرة، أرقام، إلخ)
                .NotEqual(x => x.CurrentPassword).WithMessage("New password cannot be the same as the old password.");

            RuleFor(x => x.ConfirmNewPassword)
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
        }
    }
}
