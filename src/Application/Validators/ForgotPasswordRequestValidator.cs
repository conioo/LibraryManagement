using Application.Dtos.Identity.Request;
using FluentValidation;

namespace Application.Validators
{
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(model => model.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}
