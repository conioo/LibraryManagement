#pragma warning disable CS8618
namespace Domain.Entities
{
    public class CopyHistory
    {
        public string Id { get; set; }
        public virtual ICollection<ArchivalRental>? ArchivalRentals { get; set; }
        public virtual ICollection<ArchivalReservation>? ArchivalReservations { get; set; }
    }
}