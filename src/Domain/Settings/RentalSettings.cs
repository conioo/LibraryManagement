namespace Domain.Settings
{
    public class RentalSettings
    {
        public virtual int TimeInDays { get; set; }
        public virtual decimal PenaltyChargePerDay { get; set; }
        public virtual int MaxRentalsForUser { get; set; }
        public virtual int MaxNumberOfRenewals { get; set; }
    }
}
