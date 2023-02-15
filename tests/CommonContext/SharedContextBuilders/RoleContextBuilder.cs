using WebAPI.ApiRoutes;
using WebAPITests.Integration;

namespace CommonContext.SharedContextBuilders
{
    public class RoleContextBuilder : ISharedContextBuilder
    {
        public RoleContextBuilder()
        {
            Value = new SharedContext(options =>
            {
                options.controllerPrefix = Roles.Prefix;
                options.addFakePolicyEvaluator = true;
            });
        }
        public SharedContext Value { get; }
    }
}
