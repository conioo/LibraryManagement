#pragma warning disable CS8618 

// zdjęcie

namespace Application.Dtos.Request
{
    public class ItemRequest
    {
        public string Title { get; set; }
        public Form FormOfPublication { get; set; }
        public string? Authors { get; set; }
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public int? YearOfPublication { get; set; }
        public string? ISBN { get; set; }
    }
}