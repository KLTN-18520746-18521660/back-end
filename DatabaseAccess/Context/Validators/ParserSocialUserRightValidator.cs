using FluentValidation;
using System;

namespace DatabaseAccess.Context.Validators
{
    public class ParserSocialUserRightValidator : AbstractValidator<ParserModels.ParserSocialUserRight>
    {
        public ParserSocialUserRightValidator()
        {
            RuleFor(entity => entity.right_name)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is Null")
                .NotEmpty()
                    .WithMessage("{PropertyName} is Empty")
                .Length(4, 50)
                    .WithMessage("Length of {PropertyName} must be from 4 to 50")
                .Matches("^[a-zA-Z0-9_]+$")
                    .WithMessage("{PropertyName} only accept [0-9a-zA-Z], '_'");

            RuleFor(entity => entity.display_name)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is Null")
                .NotEmpty()
                    .WithMessage("{PropertyName} is Empty")
                .Length(4, 50)
                    .WithMessage("Length of {PropertyName} must be from 4 to 50")
                .Matches("^.+$")
                    .WithMessage("{PropertyName} do not accept line terminators like: new line");
            
            RuleFor(entity => entity.describe)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is Null")
                .NotEmpty()
                    .WithMessage("{PropertyName} is Empty")
                .Length(4, 150)
                    .WithMessage("Length of {PropertyName} must be from 4 to 150")
                .Matches("^.+$")
                    .WithMessage("{PropertyName} do not accept line terminators like: new line");
        }
    }
}
