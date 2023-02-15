using WebAPI.ApiRoutes;
using WebAPITests.Integration;

namespace CommonContext.SharedContextBuilders
{
    public class AccountContextBuilder : ISharedContextBuilder
    {
        public AccountContextBuilder()
        {
            Value = new SharedContext(options =>
            {
                options.controllerPrefix = Accounts.Prefix;
                options.addEmailServiceMock = true;
            });
        }

        public SharedContext Value { get; }
    }
}
