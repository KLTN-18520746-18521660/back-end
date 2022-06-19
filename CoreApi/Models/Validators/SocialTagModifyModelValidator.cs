using FluentValidation;
using System;
using Common;
using DatabaseAccess.Context.Models;

namespace DatabaseAccess.Context.Validators
{
    public class SocialTagModifyModelValidator : AbstractValidator<ParserModels.SocialTagModifyModel>
    {
        public readonly int MinStatusLength = 1;
        public readonly int MaxStatusLength = 15;
        public readonly int MinTagNameLength = 3;
        public readonly int MaxTagNameLength = 50;
        public readonly int MinTagDescribeLength = 3;
        public readonly int MaxTagDescribeLength = 300;
        public SocialTagModifyModelValidator()
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

            When(entity => entity.name != default, () => {
                RuleFor(entity => entity.name)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Length(MinTagNameLength, MaxTagNameLength)
                        .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                            "{PropertyName}",
                            MinTagNameLength.ToString(),
                            MaxTagNameLength.ToString()
                        ));

            });

            When(entity => entity.describe != default, () => {
                RuleFor(entity => entity.describe)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Length(MinTagDescribeLength, MaxTagDescribeLength)
                        .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                            "{PropertyName}",
                            MinTagDescribeLength.ToString(),
                            MaxTagDescribeLength.ToString()
                        ));
            });
        }
    }
}
