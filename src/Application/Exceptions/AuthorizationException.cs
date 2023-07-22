namespace Application.Exceptions
{
    public class AuthorizationException : Exception
    {
        public AuthorizationException() : base() { }
        public AuthorizationException(string message) : base(message) { }
    }
}
