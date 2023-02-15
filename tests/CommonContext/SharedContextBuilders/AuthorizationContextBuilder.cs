using WebAPITests.Integration;

namespace CommonContext.SharedContextBuilders
{
    public class AuthorizationContextBuilder : ISharedContextBuilder
    {
        public AuthorizationContextBuilder()
        {
            Value = new SharedContext();
        }
        public SharedContext Value { get; }
    }
}
