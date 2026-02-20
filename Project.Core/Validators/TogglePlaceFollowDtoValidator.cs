using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using static Project.Core.DTO.CeratePlaceFollow;

namespace Project.Core.Validators
{
    public class TogglePlaceFollowDtoValidator : AbstractValidator<TogglePlaceFollowDto>
    {
        public TogglePlaceFollowDtoValidator()
        {
            RuleFor(x => x.PlaceId)
                .NotEmpty().WithMessage("معرف المكان مطلوب ولا يمكن أن يكون فارغاً.");
        }
    }
}
