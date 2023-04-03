using CommonContext.SharedContextBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.ApiRoutes;

namespace WebAPITests.Integration.SharedContextBuilders
{
    public class LibraryContextBuilder : ISharedContextBuilder
    {
        public LibraryContextBuilder()
        {
            Value = new SharedContext(options =>
            {
                options.controllerPrefix = Libraries.Prefix;
                options.addFakePolicyEvaluator = true;
                options.generateDefaultUser = true;
            });
        }
        public SharedContext Value { get; }
    }
}
