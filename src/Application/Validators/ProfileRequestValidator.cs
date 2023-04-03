using Application.Dtos.Request;
using FluentValidation;

namespace Application.Validators
{
    public class ProfileRequestValidator : AbstractValidator<ProfileRequest>
    {
        public ProfileRequestValidator()
        {
            RuleFor(model => model.PhoneNumber)
                .NotEmpty()
                .MinimumLength(9)
               .MaximumLength(30);
        }
    }
}
