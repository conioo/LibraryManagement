#pragma warning disable CS8618 

namespace Application.Dtos.Request
{
    public class CopyRequest
    {
        public string ItemId { get; set; }
        public string LibraryId { get; set; }
        public int Count { get; set; }
    }
}
