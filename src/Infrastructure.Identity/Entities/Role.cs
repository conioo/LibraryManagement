#pragma warning disable CS8618
using Domain.Interfaces;

namespace Infrastructure.Identity.Entities
{
    public class Role : IAuditableEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
