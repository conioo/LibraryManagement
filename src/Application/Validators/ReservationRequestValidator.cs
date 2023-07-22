using Application.Dtos.Request;
using FluentValidation;

namespace Application.Validators
{
    public class ReservationRequestValidator : AbstractValidator<ReservationRequest>
    {
        public ReservationRequestValidator()
        {
            RuleFor(model => model.CopyInventoryNumber)
                .NotEmpty();
        }
    }
}
