using Application.Dtos.Identity.Request;
using FluentValidation;

namespace Application.Validators
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {

            RuleFor(model => model.NewPassword)
                .NotEmpty();

            RuleFor(model => model.ConfirmNewPassword)
                .Equal(comparer => comparer.NewPassword)
                .NotEmpty();

            RuleFor(model => model.OldPassword)
                .NotEmpty();
        }
    }
}