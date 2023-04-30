#pragma warning disable CS8618
using Domain.Entities;

namespace Application.Dtos.Response.Archive
{
    public class ArchivalReservationResponse
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? CollectionDate { get; set; }
        public string? ItemTitle { get; set; }
        public string? CopyInventoryNumber { get; set; }
        public string? ProfileLibraryCardNumber { get; set; }
    }
}
