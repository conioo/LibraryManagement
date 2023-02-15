using FluentValidation;

namespace Application.Validators.Collections
{
    public class GenericCollection<T, TValidator> : AbstractValidator<IEnumerable<T>> where TValidator : IValidator<T>, new()
    {
        public GenericCollection()
        {
            RuleForEach(self => self).SetValidator(new TValidator());
        }
    }
}
