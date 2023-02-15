#pragma warning disable CS8618

namespace Application.Dtos.Identity.Request
{
    public class RoleModificationRequest
    {
        public string RoleId { get; set; }
        public IEnumerable<string> UsersId { get; set; }
    }
}
