#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class Profile : AuditableEntity
    {
        public string LibraryCardNumber { get; set; }
        public string UserId { get; set; }
        public bool IsActive { get; set; } = false;
        public string ProfileHistoryId { get; set; }
        public virtual ProfileHistory ProfileHistory { get; set; } = new ProfileHistory();
        public virtual ICollection<Rental>? CurrentRentals { get; set; }
        public virtual ICollection<Reservation>? CurrentReservations { get; set; }
    }
}