using Application.Dtos.Request;
using FluentValidation;

namespace Application.Validators
{
    public class ItemRequestValidator : AbstractValidator<ItemRequest>
    {
        public ItemRequestValidator()
        {
            RuleFor(model => model.Title)
                .MinimumLength(1);

            RuleFor(model => model.FormOfPublication)
                .IsInEnum();

            RuleFor(model => model.Description)
                .MaximumLength(600);

            RuleFor(model => model.ISBN)
                .Length(13);
        }
    }
}
