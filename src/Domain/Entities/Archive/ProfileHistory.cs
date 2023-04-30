#pragma warning disable CS8618
namespace Domain.Entities
{
    public class ProfileHistory
    {
        public string Id { get; set; }
        public virtual ICollection<ArchivalRental>? ArchivalRentals { get; set; }
        public virtual ICollection<ArchivalReservation>? ArchivalReservations { get; set; }
    }
}