using FluentValidation;
using Project.Core.DTO;

namespace Project.Core.Validators
{
    public class LoginValidator : AbstractValidator<LoginDTO>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email can't be blank")
                .EmailAddress().WithMessage("Email must be in the proper format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password can't be blank");
        }
    }
}
