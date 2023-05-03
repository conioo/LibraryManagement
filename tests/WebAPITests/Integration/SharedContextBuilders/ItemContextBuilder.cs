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
                //options.addOldFakePolicyEvaluator = true;
                options.generateDefaultUser = true;
            });
        }
        public SharedContext Value { get; }
    }
}
