using CoreApi.Models.ModifyModels;
using FluentValidation;

namespace CoreApi.Models.Validators
{
    public class SocialCommentModifyModelValidator : AbstractValidator<SocialCommentModifyModel>
    {
        public readonly int MinContentLength = 1;
        public readonly int MaxContentLength = 2000;
        public SocialCommentModifyModelValidator()
        {
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
        }
    }
}
