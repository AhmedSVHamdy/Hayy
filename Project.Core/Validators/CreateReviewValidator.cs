using FluentValidation;
using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Validators
{
    public class CreateReviewValidator : AbstractValidator<CreateReviewDto>
    {
        public CreateReviewValidator()
        {
            RuleFor(x => x.PlaceId)
                .NotEmpty().WithMessage("Place ID is required.");

            // أهم قاعدة: التقييم لازم يكون بين 1 و 5
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

            // الكومنت اختياري، بس لو اتكتب ميزدش عن 500 حرف
            RuleFor(x => x.Comment)
                .MaximumLength(500).WithMessage("Comment cannot exceed 500 characters.");
        }
    }
}
