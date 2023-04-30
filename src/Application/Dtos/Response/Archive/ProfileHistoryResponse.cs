#pragma warning disable CS8618

namespace Application.Dtos.Response.Archive
{
    public class ProfileHistoryResponse
    {
        public IEnumerable<ArchivalRentalResponse> ArchivalRentals { get; set; }
        public IEnumerable<ArchivalReservationResponse> ArchivalReservations { get; set; }
    }
}
