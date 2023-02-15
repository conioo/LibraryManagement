using Domain.Settings;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Application.Validators
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator(IOptions<UserNameSettings> userNameOptions)
        {
            var _userNameOptions = userNameOptions.Value;

            RuleFor(model => model.UserName)
              .MinimumLength(_userNameOptions.RequiredLength)
              .MaximumLength(_userNameOptions.MaximumLength);

            RuleFor(model => model.FirstName)
             .MinimumLength(1);

            RuleFor(model => model.LastName)
            .MinimumLength(1);
        }
    }
}
