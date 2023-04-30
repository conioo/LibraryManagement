#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class ArchivalRental : BaseEntity
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ReturnedDate { get; set; }
        public decimal? PenaltyCharge { get; set; }
        public virtual Profile? Profile { get; set; }
        public virtual ProfileHistory? ProfileHistory { get; set; }
        public virtual Copy? Copy { get; set; }
        public virtual CopyHistory? CopyHistory { get; set; }
    }
}
