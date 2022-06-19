using FluentValidation;
using System;
using Common;
using DatabaseAccess.Context.Models;
using System.IO;

namespace DatabaseAccess.Context.Validators
{
    public class ParserSocialCategoryValidator : AbstractValidator<ParserModels.ParserSocialCategory>
    {
        public readonly int MinNameLength = 1;
        public readonly int MaxNameLength = 20;
        public readonly int MinDisplayNameLength = 1;
        public readonly int MaxDisplayNameLength = 50;
        public readonly int MinDescribeLength = 0;
        public readonly int MaxDescribeLength = 300;
        public ParserSocialCategoryValidator()
        {
            RuleFor(entity => entity.name)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .Length(MinNameLength, MaxNameLength)
                    .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                        "{PropertyName}",
                        MinNameLength.ToString(),
                        MaxNameLength.ToString()
                    ));

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

            RuleFor(entity => entity.describe)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .Length(MinDescribeLength, MaxDescribeLength)
                    .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                        "{PropertyName}",
                        MinDescribeLength.ToString(),
                        MaxDescribeLength.ToString()
                    ));

            When(entity => entity.thumbnail != default, () => {
                RuleFor(entity => entity.thumbnail)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Must(thumbnail => Path.HasExtension(thumbnail))
                        .WithMessage("{PropertyName} is is invalid.");
            });

            When(entity => entity.parent_id != default, () => {
                RuleFor(entity => entity.parent_id)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                        .WithMessage("{PropertyName} is null.")
                    .GreaterThan(1)
                        .WithMessage("{PropertyName} is invalid.");
            });
        }
    }
}
