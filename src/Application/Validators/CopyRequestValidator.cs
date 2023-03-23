using Application.Dtos.Request;
using FluentValidation;

namespace Application.Validators
{
    public class CopyRequestValidator : AbstractValidator<CopyRequest>
    {
        public CopyRequestValidator()
        {
            RuleFor(model => model.ItemId)
                .NotEmpty();

            RuleFor(model => model.LibraryId)
                .NotEmpty();

            RuleFor(model => model.Count)
                .NotEmpty()
                .GreaterThan(0);
        }
    }
}