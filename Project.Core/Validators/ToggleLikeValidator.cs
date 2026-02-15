using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CeratePostLike;

namespace Project.Core.Validators
{
    public class ToggleLikeValidator : AbstractValidator<ToggleLikeDto>
    {
        public ToggleLikeValidator() 
        {
            RuleFor(x => x.PostId)
                .NotEmpty().WithMessage("رقم البوست (PostId) مطلوب! 🚫");

            
        }
    }
}
