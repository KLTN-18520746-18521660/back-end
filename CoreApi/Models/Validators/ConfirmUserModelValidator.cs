using FluentValidation;

namespace CoreApi.Models.Validators
{
    public class ConfirmUserModelValidator : AbstractValidator<ConfirmUserModel>
    {
        public ConfirmUserModelValidator()
        {
            RuleFor(entity => entity.password)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .MinimumLength(4)
                    .WithMessage("Length of {PropertyName} must be greater than 4.");
        }
    }
}
