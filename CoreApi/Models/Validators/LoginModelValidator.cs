using FluentValidation;

namespace CoreApi.Models.Validators
{
    public class LoginModelValidator : AbstractValidator<LoginModel>
    {
        public LoginModelValidator()
        {
            RuleFor(entity => entity.user_name)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .MinimumLength(4)
                    .WithMessage("Length of {PropertyName} must be greater than 4.");

            RuleFor(entity => entity.password)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .MinimumLength(4)
                    .WithMessage("Length of {PropertyName} must be greater than 4.");

            RuleFor(entity => entity.remember)
                .Cascade(CascadeMode.Stop)
                .Must(value => value.GetType() == typeof(bool))
                    .WithMessage("{PropertyName} must be a boolean.");

            RuleFor(entity => entity.data)
                .Cascade(CascadeMode.Stop)
                .Must(value => value == null ? true : value.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                    .WithMessage("{PropertyName} must be a Json object.");
        }
    }
}
