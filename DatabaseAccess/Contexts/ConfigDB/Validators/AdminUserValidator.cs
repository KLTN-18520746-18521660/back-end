using FluentValidation;

namespace DatabaseAccess.Contexts.ConfigDB.Validators
{
    class AdminUserValidator : AbstractValidator<Models.AdminUser>
    {
        public AdminUserValidator()
        {
            RuleFor(entity => entity.Id)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is Null")
                .NotEmpty()
                    .WithMessage("{PropertyName} is Empty");

            RuleFor(entity => entity.UserName)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is Null")
                .NotEmpty()
                    .WithMessage("{PropertyName} is Empty")
                .Length(4, 50)
                    .WithMessage("Length of user name must be from 4 to 50")
                .Matches("^[a-zA-Z0-9_]$")
                    .WithMessage("User name only accept [0-9a-zA-Z], '_'");

            RuleFor(entity => entity.DisplayName)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is Null")
                .NotEmpty()
                    .WithMessage("{PropertyName} is Empty")
                .Length(4, 50)
                    .WithMessage("Length of {PropertyName} must be from 4 to 50")
                .Matches("^.+$")
                    .WithMessage("{PropertyName} only accept [0-9a-zA-Z], '_'");

            RuleFor(entity => entity.Salt)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is Null")
                .NotEmpty()
                    .WithMessage("{PropertyName} is Empty")
                .Length(8)
                    .WithMessage("Length of {PropertyName} must be 32");

            RuleFor(entity => entity.Password)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is Null")
                .NotEmpty()
                    .WithMessage("{PropertyName} is Empty")
                .Length(32)
                    .WithMessage("Length of {PropertyName} must be 32");

            // Email Format: {64}@{255} ----------- RFC 3696 - Session 3
            // Total length: 320
            RuleFor(entity => entity.Email)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is Null")
                .NotEmpty()
                    .WithMessage("{PropertyName} is Empty")
                .MaximumLength(320)
                    .WithMessage("Total length accepted for valid email is 320. See in RFC 3696")
                .Matches(@"^[a-z0-9_\.]{1,64}@[a-z]+\.[a-z]{2,3}$")
                    .WithMessage("Email is invalid. Accept character [0-9a-zA-Z], '_'");

            RuleFor(entity => entity.Status)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is Null")
                .NotEmpty()
                    .WithMessage("{PropertyName} is Empty")
                .LessThanOrEqualTo(3).GreaterThanOrEqualTo(0)
                    .WithMessage("{PropertyName} accpet range [0, 3]");

            RuleFor(entity => entity.Rights)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is Null")
                .NotEmpty()
                    .WithMessage("{PropertyName} is Empty")
                .Must(rights => rights.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                    .WithMessage("{PropertyName} must be a Json array.");

            RuleFor(entity => entity.Settings)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is Null")
                .NotEmpty()
                    .WithMessage("{PropertyName} is Empty")
                .Must(rights => rights.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                    .WithMessage("{PropertyName} must be a Json object.");

            RuleFor(entity => entity.CreatedTimestamp)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("{PropertyName} is Null")
                .NotEmpty()
                    .WithMessage("{PropertyName} is Empty");

            RuleFor(entity => entity.LastAccessTimestamp)
                .Cascade(CascadeMode.Stop);
        }
    }
}
