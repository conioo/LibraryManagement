#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class Rental : BaseEntity
    {
        public bool Returned { get; set; } = false;
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }

        public virtual Profil Profil { get; set; }
        public virtual Copy Copy { get; set; }
    }
}
