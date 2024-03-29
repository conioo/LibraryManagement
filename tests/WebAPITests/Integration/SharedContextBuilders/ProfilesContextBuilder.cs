﻿using CommonContext.SharedContextBuilders;
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
                options.addDefaultUser = true;
                options.addProfileForDefaultUser = true;
                options.isActiveProfile = false;
            });
        }
        public SharedContext Value { get; }
    }
}