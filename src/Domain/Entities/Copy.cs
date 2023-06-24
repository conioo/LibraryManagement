#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class Copy : AuditableEntity
    {
        public string InventoryNumber { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string ItemId { get; set; }
        public string LibraryId { get; set; }
        public string CopyHistoryId { get; set; }
        public string? CurrentRentalId { get; set; }
        public string? CurrentReservationId { get; set; }
        public virtual Item Item { get; set; }
        public virtual Library Library { get; set; }
        public virtual CopyHistory CopyHistory { get; set; } = new CopyHistory();
        public virtual Rental? CurrentRental { get; set; }
        public virtual Reservation? CurrentReservation { get; set; }
    }
}