using CommonContext.SharedContextBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.ApiRoutes;
using WebAPITests.Integration;

namespace WebAPITests.Integration.SharedContextBuilders
{
    public class AdminContextBuilder : ISharedContextBuilder
    {
        public AdminContextBuilder()
        {
            Value = new SharedContext(options =>
            {
                options.controllerPrefix = Admin.Prefix;
                options.addFakePolicyEvaluator = true;
                options.addEmailServiceMock = true;
            });
        }
        public SharedContext Value { get; }
    }
}
