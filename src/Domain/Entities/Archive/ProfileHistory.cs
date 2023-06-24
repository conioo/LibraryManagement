#pragma warning disable CS8618
using Domain.Interfaces;

namespace Domain.Entities
{
    public class ProfileHistory : IHistory
    {
        public string Id { get; set; }
        public virtual Profile Profile { get; set; }
        public virtual ICollection<ArchivalRental>? ArchivalRentals { get; set; }
        public virtual ICollection<ArchivalReservation>? ArchivalReservations { get; set; }
    }
}