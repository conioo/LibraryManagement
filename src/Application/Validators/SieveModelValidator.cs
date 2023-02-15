using FluentValidation;
using Sieve.Models;

namespace Application.Validators
{
    public class SieveModelValidator : AbstractValidator<SieveModel>
    {
        public SieveModelValidator()
        {
            RuleFor(model => model.Page)
                .NotNull()
                .NotEmpty();

            RuleFor(model => model.PageSize)
                .NotNull()
                .NotEmpty();
        }
    }
}
