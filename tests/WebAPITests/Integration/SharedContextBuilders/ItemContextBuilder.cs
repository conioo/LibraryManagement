using CommonContext.SharedContextBuilders;
using WebAPI.ApiRoutes;
using WebAPITests.Integration;

namespace WebAPITests.Integration.SharedContextBuilders
{
    public class ItemContextBuilder : ISharedContextBuilder
    {
        public ItemContextBuilder()
        {
            Value = new SharedContext(options =>
            {
                options.controllerPrefix = Items.Prefix;
                options.addOldFakePolicyEvaluator = true;
            });
        }
        public SharedContext Value { get; }
    }
}
