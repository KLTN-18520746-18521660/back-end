using FluentValidation;
using System;
using Common;
using DatabaseAccess.Context.Models;
using CoreApi.Models.ModifyModels;
using System.Linq;
using System.IO;

namespace CoreApi.Models.Validators
{
    public class AdminUserRightModifyModelValidator : AbstractValidator<AdminUserRightModifyModel>
    {
        public readonly int MinDisplayNameLength = 4;
        public readonly int MaxDisplayNameLength = 50;
        public readonly int MinDescriptionLength = 4;
        public readonly int MaxDescriptionLength = 150;
        public AdminUserRightModifyModelValidator()
        {
            When(entity => entity.display_name != default, () => {
                RuleFor(entity => entity.display_name)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                        .WithMessage("{PropertyName} is null.")
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Length(MinDisplayNameLength, MaxDisplayNameLength)
                        .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                            "{PropertyName}",
                            MinDisplayNameLength.ToString(),
                            MaxDisplayNameLength.ToString()
                        ))
                    .Matches("^.+$")
                        .WithMessage("{PropertyName} do not accept line terminators like: new line");
            });

            When(entity => entity.describe != default, () => {
                RuleFor(entity => entity.describe)
                    .Cascade(CascadeMode.Stop)
                    .Length(MinDescriptionLength, MaxDescriptionLength)
                        .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                            "{PropertyName}",
                            MinDescriptionLength.ToString(),
                            MaxDescriptionLength.ToString()
                        ))
                    .Matches("^.+$")
                        .WithMessage("{PropertyName} do not accept line terminators like: new line");
            });
        }
    }
}
