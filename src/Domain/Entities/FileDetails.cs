#pragma warning disable CS8618
using Domain.Common;

namespace Domain.Entities
{
    public class FileDetails : BaseEntity
    {
        public string? FileName { get; set; }
        public byte[] FileData { get; set; }
        public FileType FileType { get; set; }
    }
}