#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class ArchivalRental : BaseEntity
    {
        public DateOnly BeginDate { get; set; }
        public DateOnly EndDate { get; set; }
        public DateOnly ReturnedDate { get; set; }
        public decimal? PenaltyCharge { get; set; }
        public int NumberOfRenewals { get; set; }
        public virtual ProfileHistory ProfileHistory { get; set; }
        public virtual CopyHistory CopyHistory { get; set; }
    }
}
