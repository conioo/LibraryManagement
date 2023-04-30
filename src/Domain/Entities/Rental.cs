#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class Rental : BaseEntity
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? PenaltyCharge { get; set; } = null;
        public virtual Profile Profile { get; set; }
        public virtual Copy Copy { get; set; }
    }
}
