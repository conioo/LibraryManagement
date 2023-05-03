#pragma warning disable CS8618
using Domain.Entities;

namespace Application.Dtos.Response
{
    public class CopyResponse
    {
        public string InventoryNumber { get; set; }
        public bool IsAvailable { get; set; }
        public string ItemId { get; set; }
        public string LibraryId { get; set; }
    }
}
