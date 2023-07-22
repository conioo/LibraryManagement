using Application.Dtos.Request;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace Application.Validators.Collections
{
    public class RentalRequestCollectionValidator : AbstractValidator<List<RentalRequest>>
    {
        public RentalRequestCollectionValidator()
        {
            RuleFor(self => self).NotEmpty().Must(self => self.DistinctBy(rental => rental.CopyInventoryNumber).Count() == self.Count());
            RuleForEach(self => self).SetValidator(new RentalRequestValidator());
        }
    }
}