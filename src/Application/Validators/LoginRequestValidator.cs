using Application.Dtos.Identity.Request;
using FluentValidation;

namespace Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(model => model.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(model => model.Password)
                .NotEmpty();
        }
    }
}
