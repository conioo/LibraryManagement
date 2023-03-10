#pragma warning disable CS8618
using Domain.Common;

namespace Domain.Entities
{
    public class Item : BaseEntity
    {
        public string Title { get; set; }
        public Form FormOfPublication { get; set; }
        public string? Authors { get; set; }
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public int? YearOfPublication { get; set; }
        public string? ISBN { get; set; }

        public string[] MyProperty { get; set; }
    }
}