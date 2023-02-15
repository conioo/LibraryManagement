using CommonContext.SharedContextBuilders;
using WebAPITests.Integration;

namespace WebAPITests.Unit
{
    public class AuthorizationTests : IClassFixture<AuthorizationContextBuilder>
    {
        private readonly SharedContext _sharedContext;

        public AuthorizationTests(AuthorizationContextBuilder contextBuilder)
        {
            _sharedContext = contextBuilder.Value;
        }
    }
}
