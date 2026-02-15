using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CeratePostComment;

namespace Project.Core.Validators
{
    public class CreatePostCommentValidator : AbstractValidator<CreateCommentDto>
    {
        public CreatePostCommentValidator()
        {
            // 1. التحقق من المحتوى (Content)
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("لا يمكن ترك التعليق فارغاً! 📝")
                .NotNull()
                .MaximumLength(1000).WithMessage("التعليق طويل جداً! الحد الأقصى 1000 حرف.");

            // 2. التحقق من وجود PostId
            RuleFor(x => x.PostId)
                .NotEmpty().WithMessage("يجب تحديد البوست المراد التعليق عليه.");

            // 3. التحقق من وجود UserId
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("يجب تحديد المستخدم صاحب التعليق.");

            
        }
    }
}
