#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class Reservation : BaseEntity
    {
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }

        public bool Received { get; set; } = false;

        public virtual Profile Profile { get; set; }
        public virtual Copy Copy { get; set; }
    }
}