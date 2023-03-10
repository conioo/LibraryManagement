using Application.Dtos.Request;
using FluentValidation;

namespace Application.Validators
{
    public class LibraryRequestValidator : AbstractValidator<LibraryRequest>
    {
        public LibraryRequestValidator()
        {
            RuleFor(model => model.Name)
                .MinimumLength(5);

            RuleFor(model => model.Address)
                .MinimumLength(10);

            RuleFor(model => model.PhoneNumber)
               .MinimumLength(9)
               .MaximumLength(30);

            RuleFor(model => model.Email)
              .EmailAddress();

            RuleFor(model => model.NumberOfComputerStations)
                .GreaterThan(-1);
        }
    }
}
