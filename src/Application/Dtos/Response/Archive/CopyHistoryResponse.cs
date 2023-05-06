#pragma warning disable CS8618

namespace Application.Dtos.Response.Archive
{
    public class CopyHistoryResponse
    {
        public IEnumerable<ArchivalRentalResponse> ArchivalRentals { get; set; }
        public IEnumerable<ArchivalReservationResponse> ArchivalReservations { get; set; }
    }
}
