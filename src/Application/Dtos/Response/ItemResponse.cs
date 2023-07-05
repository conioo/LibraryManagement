#pragma warning disable CS8618
using Application.Dtos.Request;

namespace Application.Dtos.Response
{
    public class ItemResponse
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public Form FormOfPublication { get; set; }
        public string? Authors { get; set; }
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public int? YearOfPublication { get; set; }
        public string? ISBN { get; set; }
        public ICollection<string>? ImagePaths { get; set; }
    }
}