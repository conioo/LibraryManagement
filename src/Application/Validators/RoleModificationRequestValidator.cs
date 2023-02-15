using Application.Dtos.Identity.Request;
using FluentValidation;

namespace Application.Validators
{
    public class RoleModificationRequestValidator : AbstractValidator<RoleModificationRequest>
    {
        public RoleModificationRequestValidator()
        {
            RuleFor(model => model.RoleId)
                .NotEmpty()
                .NotNull();

            RuleFor(model => model.UsersId)
                .NotEmpty()
                .NotNull();
        }
    }
}
