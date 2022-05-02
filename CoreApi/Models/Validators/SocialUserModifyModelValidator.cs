using FluentValidation;
using System;
using Common;
using DatabaseAccess.Context.Models;
using CoreApi.Models.ModifyModels;
using System.Linq;
using System.IO;

namespace CoreApi.Models.Validators
{
    public class SocialUserModifyModelValidator : AbstractValidator<SocialUserModifyModel>
    {
        public readonly int MinUserNameLength = 4;
        public readonly int MaxUserNameLength = 50;
        public readonly int MinFirstNameLength = 3;
        public readonly int MaxFirstNameLength = 25;
        public readonly int MinLastNameLength = 3;
        public readonly int MaxLastNameLength = 25;
        public readonly int MinDisplayNameLength = 4;
        public readonly int MaxDisplayNameLength = 50;
        public readonly int MaxDescriptionLength = 2048;
        public readonly string[] RequiredPublics = new string[]{
            "display_name",
            "user_name",
            "avatar",
            "status",
            "publics",
        };
        public readonly string[] OptionalPublics = new string[]{
            "email",
            "description",
            "sex",
            "country",
            "ranks",
            "followers",
            "posts",
            "views",
            "likes"
        };
        public SocialUserModifyModelValidator()
        {
            When(entity => entity.user_name != default, () => {
                RuleFor(entity => entity.user_name)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                        .WithMessage("{PropertyName} is null.")
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Length(MinUserNameLength, MaxUserNameLength)
                        .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                            "{PropertyName}",
                            MinUserNameLength.ToString(),
                            MaxUserNameLength.ToString()
                        ))
                    .Matches("^[a-zA-Z0-9_]+$")
                        .WithMessage("{PropertyName} only accept [0-9a-zA-Z], '_'.");
            });

            When(entity => entity.first_name != default, () => {
                RuleFor(entity => entity.first_name)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                        .WithMessage("{PropertyName} is null.")
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Length(MinFirstNameLength, MaxFirstNameLength)
                        .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                            "{PropertyName}",
                            MinFirstNameLength.ToString(),
                            MaxFirstNameLength.ToString()
                        ));
            });

            When(entity => entity.last_name != default, () => {
                RuleFor(entity => entity.last_name)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                        .WithMessage("{PropertyName} is null.")
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Length(MinLastNameLength, MaxLastNameLength)
                        .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                            "{PropertyName}",
                            MinLastNameLength.ToString(),
                            MaxLastNameLength.ToString()
                        ));
            });

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
                        ));
            });

            When(entity => entity.description != default, () => {
                RuleFor(entity => entity.description)
                    .Cascade(CascadeMode.Stop)
                    .MaximumLength(MaxDescriptionLength)
                        .WithMessage(string.Format("Length of {0} must be less than {1}.",
                            "{PropertyName}",
                            MaxDescriptionLength.ToString()
                        ));
            });

            When(entity => entity.email != default, () => {
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

            });

            When(entity => entity.sex != default, () => {
                RuleFor(entity => entity.sex)
                    .Cascade(CascadeMode.Stop)
                    .MaximumLength(10)
                        .WithMessage("Length of {PropertyName} must be less than 10.");
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

            When(entity => entity.ui_settings != default, () => {
                RuleFor(entity => entity.ui_settings)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                        .WithMessage("{PropertyName} is null.")
                    .Must(ui_settings => ui_settings.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                        .WithMessage("{PropertyName} must be a Json object.");
            });

            When(entity => entity.avatar != default, () => {
                RuleFor(entity => entity.avatar)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Must(avatar => Path.HasExtension(avatar))
                        .WithMessage("{PropertyName} is is invalid.");
            });

            When(entity => entity.publics != default, () => {
                RuleFor(entity => entity.publics)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                        .WithMessage("{PropertyName} is null.")
                    .Must(publics => {
                        return publics.Count(e => RequiredPublics.Contains(e)) != RequiredPublics.Length;
                    })
                        .WithMessage(
                            string.Format("{0} is missing required field. Required fields: {1}",
                                "{PropertyName}",
                                string.Join(", ", RequiredPublics)
                            )
                        )
                    .Must(publics => {
                        foreach (var p in publics) {
                            if (!OptionalPublics.Contains(p) && !RequiredPublics.Contains(p)) {
                                return false;
                            }
                        }
                        return true;
                    })
                        .WithMessage("{PropertyName} have invalid field");
            });
        }
    }
}
