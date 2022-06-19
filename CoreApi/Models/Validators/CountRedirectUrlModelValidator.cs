using FluentValidation;
using Common;

namespace CoreApi.Models.Validators
{
    public class CountRedirectUrlModelValidator : AbstractValidator<CountRedirectUrlModel>
    {
        public CountRedirectUrlModelValidator()
        {
            RuleFor(entity => entity.url)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is null.")
                .NotEmpty()
                    .WithMessage("{PropertyName} is empty.")
                .Must(value => CommonValidate.IsValidUrl(value, true) == true)
                    .WithMessage("Invalid Url");
        }
    }
}
