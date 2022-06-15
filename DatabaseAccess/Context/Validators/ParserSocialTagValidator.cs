using FluentValidation;
using System;
using Common;
using DatabaseAccess.Context.Models;

namespace DatabaseAccess.Context.Validators
{
    public class ParserSocialTagValidator : AbstractValidator<ParserModels.ParserSocialTag>
    {
        public readonly int MinTagLength = 3;
        public readonly int MaxTagLength = 25;
        public readonly int MinTagNameLength = 3;
        public readonly int MaxTagNameLength = 50;
        public readonly int MinTagDescribeLength = 3;
        public readonly int MaxTagDescribeLength = 300;
        public ParserSocialTagValidator()
        {
            RuleFor(entity => entity.tag)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .Length(MinTagLength, MaxTagLength)
                    .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                        "{PropertyName}",
                        MinTagLength.ToString(),
                        MaxTagLength.ToString()
                    ))
                .Matches("^[a-zA-Z0-9-_]+$")
                    .WithMessage("{PropertyName} only accept [0-9a-zA-Z], '_', '-'.");

            RuleFor(entity => entity.name)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .Length(MinTagNameLength, MaxTagNameLength)
                    .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                        "{PropertyName}",
                        MinTagNameLength.ToString(),
                        MaxTagNameLength.ToString()
                    ));

            RuleFor(entity => entity.describe)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .Length(MinTagDescribeLength, MaxTagDescribeLength)
                    .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                        "{PropertyName}",
                        MinTagDescribeLength.ToString(),
                        MaxTagDescribeLength.ToString()
                    ));
        }
    }
}
