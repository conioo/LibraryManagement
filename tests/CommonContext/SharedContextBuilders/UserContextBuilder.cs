using WebAPI.ApiRoutes;
using WebAPITests.Integration;

namespace CommonContext.SharedContextBuilders
{
    public class UserContextBuilder : ISharedContextBuilder
    {
        public UserContextBuilder()
        {
            Value = new SharedContext(options =>
            {
                options.controllerPrefix = Users.Prefix;
                options.addFakePolicyEvaluator = true;
            });
        }
        public SharedContext Value { get; }
    }
}
