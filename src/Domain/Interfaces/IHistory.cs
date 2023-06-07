using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IHistory
    {
        public ICollection<ArchivalRental>? ArchivalRentals { get; set; }
        public ICollection<ArchivalReservation>? ArchivalReservations { get; set; }
    }
}
