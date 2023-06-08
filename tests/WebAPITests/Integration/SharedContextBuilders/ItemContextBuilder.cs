using CommonContext.SharedContextBuilders;
using WebAPI.ApiRoutes;

namespace WebAPITests.Integration.SharedContextBuilders
{
    public class ItemContextBuilder : ISharedContextBuilder
    {
        public ItemContextBuilder()
        {
            Value = new SharedContext(options =>
            {
                options.controllerPrefix = Items.Prefix;
                options.addFakePolicyEvaluator = true;
                options.addDefaultUser = true;
            });
        }

        public SharedContext Value { get; }
    }
}
