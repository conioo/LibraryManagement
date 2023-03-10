using CommonContext.SharedContextBuilders;
using WebAPI.ApiRoutes;
using WebAPITests.Integration;

namespace WebAPITests.Integration.SharedContextBuilders
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
