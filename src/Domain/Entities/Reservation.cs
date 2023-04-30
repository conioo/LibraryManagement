#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class Reservation : BaseEntity
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual Profile Profile { get; set; }
        public virtual Copy Copy { get; set; }
    }
}