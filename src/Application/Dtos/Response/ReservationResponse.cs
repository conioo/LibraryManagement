#pragma warning disable CS8618

namespace Application.Dtos.Response
{
    public class ReservationResponse
    {
        public string Id { get; set; }
        public DateOnly BeginTime { get; set; }
        public DateOnly EndTime { get; set; }
        public string? ItemTitle { get; set; }
        public string CopyInventoryNumber { get; set; }
        public string ProfileLibraryCardNumber { get; set; }
    }
}
