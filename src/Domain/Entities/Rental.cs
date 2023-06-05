#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class Rental : BaseEntity
    {
        public DateOnly BeginDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int NumberOfRenewals { get; set; } = 0;
        public decimal? PenaltyCharge { get; set; } = null;
        public virtual Profile Profile { get; set; }
        public virtual Copy Copy { get; set; }
    }
}