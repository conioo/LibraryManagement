namespace CommonContext
{
    public class SharedContextOptions
    {
        public string controllerPrefix = "";

        public bool addFakePolicyEvaluator = false;

        public bool addOldFakePolicyEvaluator = false;

        public bool addEmailServiceMock = false;

        public bool addCountingOfPenaltyChargesMock = false;

        public bool addEndOfReservationMock = false;

        public bool addDefaultUser = false;

        public bool addProfileForDefaultUser = false;

        public bool removeRentalsAndReservationsForTheProfile = false;

        public bool isActiveProfile = true;
    }
}
