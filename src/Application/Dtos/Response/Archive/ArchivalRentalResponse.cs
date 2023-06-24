#pragma warning disable CS8618
using Domain.Entities;

namespace Application.Dtos.Response.Archive
{
    public class ArchivalRentalResponse
    {
        public DateOnly BeginDate { get; set; }
        public DateOnly EndDate { get; set; }
        public DateOnly ReturnedDate { get; set; }
        public decimal? PenaltyCharge { get; set; }
        public string ItemTitle { get; set; }
        public string CopyInventoryNumber { get; set; }
        public string ProfileLibraryCardNumber { get; set; }
    }
}
