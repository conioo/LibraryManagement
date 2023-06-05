using FluentValidation;

namespace Application.Validators.Collections
{
    public class StringCollectionValidator : AbstractValidator<List<string>>
    {
        public StringCollectionValidator()
        {
            RuleForEach(self => self).NotEmpty();
        }
    }
}