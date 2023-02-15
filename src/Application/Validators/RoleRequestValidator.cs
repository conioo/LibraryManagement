using Application.Dtos.Identity.Request;
using FluentValidation;

namespace Application.Validators
{
    public class RoleRequestValidator : AbstractValidator<RoleRequest>
    {
        public RoleRequestValidator()
        {
            RuleFor(model => model.Name)
                .NotEmpty()
                .MinimumLength(3);
        }
    }
}
