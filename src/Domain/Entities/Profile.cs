#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class Profile : AuditableEntity
    {
        public string LibraryCardNumber { get; set; }
        public string UserId { get; set; }

        public bool IsActive { get; set; } = false;

        public History? History { get; set; }
    }
}