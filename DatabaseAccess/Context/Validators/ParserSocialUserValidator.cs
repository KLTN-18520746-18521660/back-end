using Common;
using FluentValidation;
using System;

namespace DatabaseAccess.Context.Validators
{
    public class ParserSocialUserValidator : AbstractValidator<ParserModels.ParserSocialUser>
    {
        public ParserSocialUserValidator()
        {
            // When(entity => entity.user_name != default, () => {
            //     RuleFor(entity => entity.user_name)
            //         .Cascade(CascadeMode.Stop)
            //         .NotNull()
            //             .WithMessage("{PropertyName} is null.")
            //         .NotEmpty()
            //             .WithMessage("{PropertyName} is empty.")
            //         .Length(4, 50)
            //             .WithMessage("Length of {PropertyName} must be from 4 to 50.")
            //         .Matches("^[a-zA-Z0-9_]+$")
            //             .WithMessage("{PropertyName} only accept [0-9a-zA-Z], '_'.");
            // });

            RuleFor(entity => entity.first_name)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .Length(4, 25)
                    .WithMessage("Length of {PropertyName} must be from 4 to 25.");

            RuleFor(entity => entity.last_name)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .Length(4, 25)
                    .WithMessage("Length of {PropertyName} must be from 4 to 25.");

            When(entity => entity.display_name != default, () => {
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
            });

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
                .Matches("^[a-z0-9_\\.]{1,64}@[a-z]+\\.[a-z]{2,3}$")
                    .WithMessage("Email is invalid.");

            When(entity => entity.sex != default, () => {
                RuleFor(entity => entity.sex)
                    .Cascade(CascadeMode.Stop)
                    .MaximumLength(10)
                        .WithMessage("Length of {PropertyName} must be equals or less than 10.");
            });

            When(entity => entity.phone != default, () => {
                RuleFor(entity => entity.phone)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                        .WithMessage("{PropertyName} is null.")
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Length(8, 20)
                        .WithMessage("Length of {PropertyName} must be from 8 to 20.")
                    .Matches("^[+]{0,1}[0-9]+$")
                        .WithMessage("{PropertyName} only accept [0-9], '+'.");
            });

            When(entity => entity.city != default, () => {
                RuleFor(entity => entity.city)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                        .WithMessage("{PropertyName} is null.")
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .MaximumLength(20)
                        .WithMessage("Length of {PropertyName} must be equals or less than 20.");
            });

            When(entity => entity.province != default, () => {
                RuleFor(entity => entity.province)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                        .WithMessage("{PropertyName} is null.")
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .MaximumLength(20)
                        .WithMessage("Length of {PropertyName} must be equals or less than 10.");
            });

            When(entity => entity.country != default, () => {
                RuleFor(entity => entity.country)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                        .WithMessage("{PropertyName} is null.")
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .MaximumLength(20)
                        .WithMessage("Length of {PropertyName} must be equals or less than 20.");
            });

            When(entity => entity.settings != default, () => {
                RuleFor(entity => entity.settings)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                        .WithMessage("{PropertyName} is null.")
                    .Must(rights => rights.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                        .WithMessage("{PropertyName} must be a Json object.");
            });

            When(entity => entity.avatar != default, () => {
                RuleFor(entity => entity.avatar)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Must(thumbnail => CommonValidate.IsValidUrl(thumbnail))
                        .WithMessage("{PropertyName} is is invalid.");

            });
        }
    }
}
