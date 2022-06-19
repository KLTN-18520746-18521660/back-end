using FluentValidation;
using System;

namespace DatabaseAccess.Context.Validators
{
    public class ParserAdminBaseConfigValidator : AbstractValidator<ParserModels.ParserAdminBaseConfig>
    {
        public ParserAdminBaseConfigValidator()
        {
            RuleFor(entity => entity.config_key)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is Null")
                .NotEmpty()
                    .WithMessage("{PropertyName} is Empty")
                .Length(4, 50)
                    .WithMessage("Length of {PropertyName} must be from 4 to 50")
                .Matches("^[a-zA-Z0-9_]+$")
                    .WithMessage("{PropertyName} only accept [0-9a-zA-Z], '_'");

            RuleFor(entity => entity.value)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is Null")
                .NotEmpty()
                    .WithMessage("{PropertyName} is Empty")
                .Must(value => value.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                    .WithMessage("{PropertyName} must be a Json object.");
        }
    }
}
