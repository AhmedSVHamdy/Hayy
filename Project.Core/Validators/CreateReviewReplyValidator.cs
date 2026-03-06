using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CreateReviewReplyDTO;

namespace Project.Core.Validators
{
    public class CreateReviewReplyValidator : AbstractValidator<CreateReviewReplyDto>
    {
        public CreateReviewReplyValidator()
        {
            RuleFor(x => x.ReviewId)
                .NotEmpty().WithMessage("لازم تحدد الريفيو اللي بترد عليه.");

            RuleFor(x => x.ReplyText)
                .NotEmpty().WithMessage("محتوى الرد مطلوب.")
                .MaximumLength(1000).WithMessage("الرد طويل جداً، أقصى حد 1000 حرف.");
        }
    }
}
