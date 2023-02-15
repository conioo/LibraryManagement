using Application.Dtos.Request;
using FluentValidation;

namespace Application.Validators.Collections
{
    public class ItemRequestCollectionValidator : AbstractValidator<IEnumerable<ItemRequest>>
    {
        public ItemRequestCollectionValidator()
        {
            RuleForEach(self => self).SetValidator(new ItemRequestValidator());
        }
    }
}
