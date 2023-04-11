#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class Rental : BaseEntity
    {
        public bool IsReturned { get; set; } = false;
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }

        public virtual Profile Profile { get; set; }
        public virtual Copy Copy { get; set; }

        public string ProfileLibraryCardNumber { get; set; }
        public string CopyInventoryNumber { get; set; }

        public decimal? PenaltyCharge { get; set; } = null;
    }
}
