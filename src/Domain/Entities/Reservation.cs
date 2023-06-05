#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class Reservation : BaseEntity
    {
        public DateOnly BeginDate { get; set; }
        public DateOnly EndDate { get; set; }
        public virtual Profile Profile { get; set; }
        public virtual Copy Copy { get; set; }
    }
}