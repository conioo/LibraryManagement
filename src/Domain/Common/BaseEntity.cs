#pragma warning disable CS8618 

using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Common
{
    public abstract class BaseEntity : AuditableEntity
    {
        public string Id { get; set; }
    }
}
