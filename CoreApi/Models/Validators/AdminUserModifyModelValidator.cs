using FluentValidation;
using System;
using Common;
using DatabaseAccess.Context.Models;
using CoreApi.Models.ModifyModels;
using System.Linq;
using System.IO;

namespace CoreApi.Models.Validators
{
    public class AdminUserModifyModelValidator : AbstractValidator<AdminUserModifyModel>
    {
        public readonly int MinDisplayNameLength = 4;
        public readonly int MaxDisplayNameLength = 50;
        public AdminUserModifyModelValidator()
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

            When(entity => entity.roles != default, () => {
                RuleFor(entity => entity.roles)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                        .WithMessage("{PropertyName} is Null")
                    .Must(rights => rights.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                        .WithMessage("{PropertyName} must be a Json array.")
                    .Must((entity, rights) => entity.IsValidRights())
                        .WithMessage("{PropertyName} is invalid.");
            });
        }
    }
}
