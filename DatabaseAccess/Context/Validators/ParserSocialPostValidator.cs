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
                    ));

            When(entity => entity.thumbnail != default, () => {
                RuleFor(entity => entity.thumbnail)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Must(thumbnail => CommonValidate.IsValidUrl(thumbnail))
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

            When(entity => entity.time_read != default, () => {
                RuleFor(entity => entity.time_read)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Must(time_read => time_read >= 2 && time_read <= 100)
                        .WithMessage("{PropertyName} is must in range [2, 100].");
            });

            RuleFor(entity => entity.content_type)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .Must(content_type => SocialPost.StringToContentType(content_type) != CONTENT_TYPE.INVALID)
                    .WithMessage("{PropertyName} is invalid.");

            RuleFor(entity => entity.categories)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .Must(categories => categories.Length > 0)
                    .WithMessage("{PropertyName} must belong to at least one category.");
        }
    }
}
