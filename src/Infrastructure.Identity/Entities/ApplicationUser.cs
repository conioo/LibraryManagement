#pragma warning disable CS8618
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Entities
{
    public class ApplicationUser : IdentityUser, IAuditableEntity
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public RefreshToken? RefreshToken { get; set; }
        public string? ProfileId { get; set; } = null;
        public string? RefreshTokenId { get; set; }
        public DateTime Created { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
