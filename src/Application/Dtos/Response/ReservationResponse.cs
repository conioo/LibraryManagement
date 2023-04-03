#pragma warning disable CS8618

namespace Application.Dtos.Response
{
    public class ReservationResponse
    {
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }

        public bool Received { get; set; }

        public string? ItemTitle { get; set; }

        public string CopyInventoryNumber { get; set; }
        public string ProfileLibraryCardNumber { get; set; }
    }
}
