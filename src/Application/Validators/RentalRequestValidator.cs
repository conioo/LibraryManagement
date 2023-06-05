using Application.Dtos.Request;
using FluentValidation;

namespace Application.Validators
{
    public class RentalRequestValidator : AbstractValidator<RentalRequest>
    {
        public RentalRequestValidator()
        {
            RuleFor(model => model.CopyInventoryNumber)
                .NotEmpty();
        }
    }
}
