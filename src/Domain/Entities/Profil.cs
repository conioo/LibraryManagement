#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class Profil : AuditableEntity
    {
        public string LibraryCardNumber { get; set; }
        public string UserId { get; set; }

        public ICollection<Rental>? HistoryRentals { get; set; }
        public ICollection<Reservation>? HistoryReservations { get; set; }

    }
}
