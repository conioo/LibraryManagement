using CommonContext.SharedContextBuilders;
using WebAPI.ApiRoutes;

namespace WebAPITests.Integration.SharedContextBuilders
{
    public class CopiesContextBuilder : ISharedContextBuilder
    {
        public CopiesContextBuilder()
        {
            Value = new SharedContext(options =>
            {
                options.controllerPrefix = Copies.Prefix;
                options.addFakePolicyEvaluator = true;
                options.addDefaultUser = true;
            });

        }

        public SharedContext Value { get; }
    }
}
