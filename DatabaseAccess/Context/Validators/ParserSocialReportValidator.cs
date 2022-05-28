using FluentValidation;
using System;
using Common;
using DatabaseAccess.Context.Models;

namespace DatabaseAccess.Context.Validators
{
    public class ParserSocialReportValidator : AbstractValidator<ParserModels.ParserSocialReport>
    {
        public readonly int MinReportTypeLength = 1;
        public readonly int MaxReportTypeLength = 100;
        public readonly int MinContentLength = 1;
        public readonly int MaxContentLength = 2000;
        public ParserSocialReportValidator()
        {
            RuleFor (entity => entity.report_type)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .Length(MinReportTypeLength, MaxReportTypeLength)
                    .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                        "{PropertyName}",
                        MinReportTypeLength.ToString(),
                        MaxReportTypeLength.ToString()
                    ));
            
            When(entity => entity.report_type.ToLower() != "others", () => {
                RuleFor(entity => entity.content)
                    .Cascade(CascadeMode.Stop)
                    .Must(value => value == default || value == string.Empty)
                        .WithMessage("{PropertyName} is must be empty.");
            });
            When(entity => entity.report_type.ToLower() == "others", () => {
                RuleFor(entity => entity.content)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                        .WithMessage("{PropertyName} is null.")
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Length(MinContentLength, MaxContentLength)
                        .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                            "{PropertyName}",
                            MinContentLength.ToString(),
                            MaxContentLength.ToString()
                        ));
            });
            When(entity => entity.user_name != default, () => {
                RuleFor(entity => entity.user_name)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Length(MinContentLength, MaxContentLength)
                        .WithMessage(string.Format("Length of {0} must be from {1} to {2}.",
                            "{PropertyName}",
                            4.ToString(),
                            50.ToString()
                        ));
            });
            When(entity => entity.post_slug != default, () => {
                RuleFor(entity => entity.user_name)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.");
            });
            When(entity => entity.comment_id != default, () => {
                RuleFor(entity => entity.comment_id)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage("{PropertyName} is empty.")
                    .Must(value => value > 0)
                        .WithMessage("{PropertyName} is invalid.");
            });
        }
    }
}
