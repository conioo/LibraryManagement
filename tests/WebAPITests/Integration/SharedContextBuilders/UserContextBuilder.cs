using CommonContext.SharedContextBuilders;
using WebAPI.ApiRoutes;
using WebAPITests.Integration;

namespace WebAPITests.Integration.SharedContextBuilders
{
    public class UserContextBuilder : ISharedContextBuilder
    {
        public UserContextBuilder()
        {
            Value = new SharedContext(options =>
            {
                options.controllerPrefix = Users.Prefix;
                options.addFakePolicyEvaluator = true;
                options.addDefaultUser = true;
            });
        }
        public SharedContext Value { get; }
    }
}
