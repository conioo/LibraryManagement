using Application.Dtos.Identity.Request;
using Domain.Settings;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Application.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator(IOptions<UserNameSettings> userNameOptions)
        {
            var _userNameOptions = userNameOptions.Value;

            RuleFor(model => model.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(model => model.Password)
                .NotEmpty();

            RuleFor(model => model.ConfirmPassword)
                .NotEmpty()
                .Equal(comparer => comparer.Password);

            RuleFor(model => model.UserName)
                .MinimumLength(_userNameOptions.RequiredLength)
                .MaximumLength(_userNameOptions.MaximumLength)
                .NotEmpty();
        }
    }
}
