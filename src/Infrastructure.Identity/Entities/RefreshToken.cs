#pragma warning disable CS8618 

namespace Infrastructure.Identity.Entities
{
    public class RefreshToken
    {
        public string Id { get; set; }
        public DateTime Created { get; set; }
        public string Token { get; set; }
        public DateTime Expired { get; set; }
        public bool Revoke { get; set; } = false;
        public bool IsExpired => DateTime.UtcNow >= Expired;
        public bool IsActive => !Revoke && !IsExpired;
        public ApplicationUser ApplicationUser { get; set; }
    }
}
