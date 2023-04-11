#pragma warning disable CS8618
namespace Domain.Entities
{
    public class History
    {
        public string Id { get; set; }
        public ICollection<Rental>? HistoryRentals { get; set; }
        public ICollection<Reservation>? HistoryReservations { get; set; }
    }
}
