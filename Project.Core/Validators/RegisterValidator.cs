using FluentValidation;
using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterValidator()
        {
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email can't be blank")
                    .EmailAddress().WithMessage("Email must be in proper format");

                RuleFor(x => x.FullName)
                    .NotEmpty().WithMessage("Name can't be blank")
                    .MinimumLength(3).WithMessage("Name must be at least 3 characters");

                RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Password can't be blank")
                    .MinimumLength(6).WithMessage("Password must be at least 6 characters"); // إضافة طول للأمان

                RuleFor(x => x.ConfirmPassword)
                    .NotEmpty().WithMessage("Confirm Password can't be blank")
                    .Equal(x => x.Password).WithMessage("Password and Confirm Password do not match");

            }
        }







    }
    

