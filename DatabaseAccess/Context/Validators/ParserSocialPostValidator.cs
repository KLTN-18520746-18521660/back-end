using FluentValidation;
using System;
using Common;
using DatabaseAccess.Context.Models;

namespace DatabaseAccess.Context.Validators
{
    public class ParserSocialPostValidator : AbstractValidator<ParserModels.ParserSocialPost>
    {
        public readonly int MinTitleLength = 20;
        public readonly int MaxTitleLength = 200;
        public readonly int MinShortTitleLength = 20;
        public readonly int MaxShortTitleLength = 200;
        public ParserSocialPostValidator()
        {
            RuleFor(entity => entity.title)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .Length(MinTitleLength, MaxTitleLength)
                    .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                        "{PropertyName}",
                        MinTitleLength.ToString(),
                        MaxTitleLength.ToString()
                    ))
                .Matches("^[a-zA-Z0-9_\\s]+$")
                    .WithMessage("{PropertyName} only accept [0-9a-zA-Z], '_'.");

            When(entity => entity.thumbnail != default, () => {
                RuleFor(entity => entity.thumbnail)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Must(thumbnail => Utils.IsValidUrl(thumbnail))
                        .WithMessage("{PropertyName} is is invalid.");

            });

            RuleFor(entity => entity.content)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.");

            When(entity => entity.short_content != default, () => {
                RuleFor(entity => entity.short_content)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Length(MinShortTitleLength, MaxShortTitleLength)
                        .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                            "{PropertyName}",
                            MinShortTitleLength.ToString(),
                            MaxShortTitleLength.ToString()
                        ));
            });

            RuleFor(entity => entity.content_type)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .Must(content_type => SocialPost.StringToContentType(content_type) != CONTENT_TYPE.INVALID)
                    .WithMessage("{PropertyName} is invalid.");
        }
    }
}
