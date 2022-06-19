using FluentValidation;
using System;
using Common;
using DatabaseAccess.Context.Models;
using CoreApi.Models.ModifyModels;
using System.Linq;
using System.IO;

namespace CoreApi.Models.Validators
{
    public class SocialUserModifyModelByAdminValidator : AbstractValidator<SocialUserModifyModelByAdmin>
    {
        public SocialUserModifyModelByAdminValidator()
        {
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
