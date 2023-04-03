using Domain.Entities;

namespace Application.Dtos.Response
{
    public class ProfileResponse
    {
        public string LibraryCardNumber { get; set; }
        public string UserId { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<RentalResponse>? HistoryRentals { get; set; }
        public IEnumerable<ReservationResponse>? HistoryReservations { get; set; }
    }
}
