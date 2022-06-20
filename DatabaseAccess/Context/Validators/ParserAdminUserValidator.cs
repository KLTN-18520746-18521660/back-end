using FluentValidation;
using System;

namespace DatabaseAccess.Context.Validators
{
    public class ParserAdminUserValidator : AbstractValidator<ParserModels.ParserAdminUser>
    {
        public ParserAdminUserValidator()
        {
            RuleFor(entity => entity.user_name)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .Length(4, 50)
                    .WithMessage("Length of {PropertyName} must be from 4 to 50.")
                .Matches("^[a-zA-Z0-9_]+$")
                    .WithMessage("{PropertyName} only accept [0-9a-zA-Z], '_'.");

            RuleFor(entity => entity.display_name)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .Length(4, 50)
                    .WithMessage("Length of {PropertyName} must be from 4 to 50.")
                .Matches("^.+$")
                    .WithMessage("{PropertyName} only accept [0-9a-zA-Z], '_'.");

            RuleFor(entity => entity.password)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.");

            // Email Format: {64}@{255} ----------- RFC 3696 - Session 3
            // Total length: 320
            RuleFor(entity => entity.email)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .MaximumLength(320)
                    .WithMessage("Total length accepted for valid email is 320. See in RFC 3696.")
                .Matches("^[a-z0-9_\\.]{1,64}@[a-z\\.]+\\.[a-z]{2,3}$")
                    .WithMessage("Email is invalid.");

            RuleFor(entity => entity.settings)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .Must(settings => settings.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                    .WithMessage("{PropertyName} must be a Json object.");
        }
    }
}
