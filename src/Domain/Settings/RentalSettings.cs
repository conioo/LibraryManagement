namespace Domain.Settings
{
    public class RentalSettings
    {
        public int TimeInDays { get; set; }
        public decimal PenaltyChargePerDay { get; set; }
        public int MaxRentalsForUser { get; set; }
        public int MaxNumberOfRenewals { get; set; }
    }
}
