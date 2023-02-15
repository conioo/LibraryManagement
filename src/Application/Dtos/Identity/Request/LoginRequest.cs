#pragma warning disable CS8618 

namespace Application.Dtos.Identity.Request
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
