using CommonContext.SharedContextBuilders;
using WebAPI.ApiRoutes;

namespace WebAPITests.Integration.SharedContextBuilders
{
    public class ProfilesContextBuilder : ISharedContextBuilder
    {
        public ProfilesContextBuilder()
        {
            Value = new SharedContext(options =>
            {
                options.controllerPrefix = Profiles.Prefix;
                options.addFakePolicyEvaluator = true;
            });
        }
        public SharedContext Value { get; }
    }
}
