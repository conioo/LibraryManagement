using CommonContext.SharedContextBuilders;
using WebAPI.ApiRoutes;

namespace WebAPITests.Integration.SharedContextBuilders
{
    public class RentalContextBuilder : ISharedContextBuilder
    {
        public RentalContextBuilder()
        {
            Value = new SharedContext(options =>
            {
                options.controllerPrefix = Rentals.Prefix;
                options.addDefaultUser = true;
                options.addProfileForDefaultUser = true;
                options.removeRentalsAndReservationsForTheProfile = true;
                options.addFakePolicyEvaluator = true;
                options.addCountingOfPenaltyChargesMock = true;
            });
        }
        public SharedContext Value { get; }
    }
}
