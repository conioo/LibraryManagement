using CommonContext.SharedContextBuilders;
using WebAPI.ApiRoutes;

namespace WebAPITests.Integration.SharedContextBuilders
{
    public class ReservationContextBuilder : ISharedContextBuilder
    {
        public ReservationContextBuilder()
        {
            Value = new SharedContext(options =>
            {
                options.controllerPrefix = Reservations.Prefix;
                options.addDefaultUser = true;
                options.addProfileForDefaultUser = true;
                options.removeRentalsAndReservationsForTheProfile = true;
                options.addFakePolicyEvaluator = true;
                options.addEndOfReservationMock = true;
                options.addCountingOfPenaltyChargesMock = true;
            });
        }
        public SharedContext Value { get; }
    }
}
