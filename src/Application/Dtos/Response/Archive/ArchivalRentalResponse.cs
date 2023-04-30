#pragma warning disable CS8618
using Domain.Entities;

namespace Application.Dtos.Response.Archive
{
    public class ArchivalRentalResponse
    {

        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ReturnedDate { get; set; }
        public decimal? PenaltyCharge { get; set; }
        public string? ItemTitle { get; set; }
        public string? CopyInventoryNumber { get; set; }
        public string? ProfileLibraryCardNumber { get; set; }
    }
}
