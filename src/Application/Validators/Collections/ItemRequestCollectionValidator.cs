using Application.Dtos.Request;
using FluentValidation;

namespace Application.Validators.Collections
{
    public class ItemRequestCollectionValidator : AbstractValidator<List<ItemRequest>>
    {
        public ItemRequestCollectionValidator()
        {
            RuleFor(self => self).NotEmpty();
            RuleForEach(self => self).SetValidator(new ItemRequestValidator());
        }
    }
}
