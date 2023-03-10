using FluentValidation;
using Sieve.Models;

namespace Application.Validators
{
    public class SieveModelValidator : AbstractValidator<SieveModel>
    {
        public SieveModelValidator()
        {
            RuleFor(model => model.Page)
                .NotEmpty();

            RuleFor(model => model.PageSize)
                .NotEmpty();
        }
    }
}
