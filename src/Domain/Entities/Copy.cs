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
        public virtual CopyHistory CopyHistory { get; set; }
        public virtual Rental? CurrentRental { get; set; }
        public virtual string? CurrentRentalId { get; set; }
        public virtual Reservation? CurrentReservation { get; set; }
        public virtual string? CurrentReservationId { get; set; }
    }
}