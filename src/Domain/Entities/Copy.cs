#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class Copy : AuditableEntity
    {
        public string InventoryNumber { get; set; }
        public bool IsAvailable { get; set; } = true;
        public virtual Item Item { get; set; }
        public virtual Library Library { get; set; }
        public virtual History? History { get; set; }
        public virtual Rental? LastRental { get; set; }
        public virtual string? LastRentalId { get; set; }
        public virtual Reservation? LastReservation { get; set; }
        public virtual string? LastReservationId { get; set; }
    }
}
