using Application.Dtos.Identity.Request;
using FluentValidation;

namespace Application.Validators
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(model => model.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(model => model.NewPassword)
                .NotEmpty();

            RuleFor(model => model.ConfirmNewPassword)
                .NotEmpty()
                .Equal(comparer => comparer.NewPassword);

            RuleFor(model => model.PasswordResetToken)
                .NotEmpty();
        }
    }
}