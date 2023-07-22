using Application.Dtos.Request;
using FluentValidation;

namespace Application.Validators.Collections
{
    public class ReservationRequestCollectionValidator : AbstractValidator<List<ReservationRequest>>
    {
        public ReservationRequestCollectionValidator()
        {
            RuleFor(self => self).NotEmpty().Must(self => self.DistinctBy(rental => rental.CopyInventoryNumber).Count() == self.Count());
            RuleForEach(self => self).SetValidator(new ReservationRequestValidator());
        }
    }
}