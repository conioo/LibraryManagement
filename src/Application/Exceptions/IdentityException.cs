using Microsoft.AspNetCore.Identity;

namespace Application.Exceptions
{
    public class IdentityException : Exception
    {
        public IdentityException() { }
        public IdentityException(string message) : base(message) { }
        public IdentityException(string message, Exception innerException) : base(message, innerException) { }
        public IdentityException(IEnumerable<IdentityError> errors)
        {
            Errors = errors;
        }

        public IEnumerable<IdentityError>? Errors { get; set; }
    }
}
