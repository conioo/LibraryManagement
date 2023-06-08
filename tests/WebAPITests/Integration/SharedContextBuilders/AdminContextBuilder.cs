using CommonContext.SharedContextBuilders;
using WebAPI.ApiRoutes;

namespace WebAPITests.Integration.SharedContextBuilders
{
    public class AdminContextBuilder : ISharedContextBuilder
    {
        public AdminContextBuilder()
        {
            Value = new SharedContext(options =>
            {
                options.controllerPrefix = Admin.Prefix;
                options.addFakePolicyEvaluator = true;
                options.addEmailServiceMock = true;
                options.addDefaultUser = true;
            });
        }

        public SharedContext Value { get; }
    }
}
