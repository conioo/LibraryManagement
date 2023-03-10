using CommonContext.SharedContextBuilders;
using WebAPI.ApiRoutes;

namespace WebAPITests.Integration.SharedContextBuilders
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
