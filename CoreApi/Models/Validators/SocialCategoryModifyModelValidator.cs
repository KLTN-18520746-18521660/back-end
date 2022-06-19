using FluentValidation;
using System;
using Common;
using DatabaseAccess.Context.Models;
using System.IO;

namespace DatabaseAccess.Context.Validators
{
    public class SocialCategoryModifyModelValidator : AbstractValidator<ParserModels.SocialCategoryModifyModel>
    {
        public readonly int MinStatusLength = 1;
        public readonly int MaxStatusLength = 15;
        public readonly int MinDisplayNameLength = 1;
        public readonly int MaxDisplayNameLength = 50;
        public readonly int MinDescribeLength = 0;
        public readonly int MaxDescribeLength = 300;
        public SocialCategoryModifyModelValidator()
        {
            When(entity => entity.status != default, () => {
                RuleFor(entity => entity.status)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Length(MinStatusLength, MaxStatusLength)
                        .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                            "{PropertyName}",
                            MinStatusLength.ToString(),
                            MaxStatusLength.ToString()
                        ));
            });

            When(entity => entity.display_name != default, () => {
                RuleFor(entity => entity.display_name)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Length(MinDisplayNameLength, MaxDisplayNameLength)
                        .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                            "{PropertyName}",
                            MinDisplayNameLength.ToString(),
                            MaxDisplayNameLength.ToString()
                        ));
            });

            When(entity => entity.describe != default, () => {
                RuleFor(entity => entity.describe)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Length(MinDescribeLength, MaxDescribeLength)
                        .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                            "{PropertyName}",
                            MinDescribeLength.ToString(),
                            MaxDescribeLength.ToString()
                        ));
            });

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
