using FluentValidation;
using Project.Core.DTO;
using Project.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Validators
{
    public class CreateUserLogValidator : AbstractValidator<CreateUserLogDto>
    {
        public CreateUserLogValidator()
        {
            // 1. القواعد الأساسية
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.ActionType)
                .IsInEnum().WithMessage("Invalid Action Type.");

            RuleFor(x => x.Duration)
                .GreaterThanOrEqualTo(0).WithMessage("Duration cannot be negative.");

            
        }
    }
}
