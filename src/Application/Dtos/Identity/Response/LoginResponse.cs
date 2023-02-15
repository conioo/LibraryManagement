#pragma warning disable CS8618

namespace Application.Dtos.Identity.Response
{
    public class LoginResponse
    {
        public string UserId { get; set; }
        public string Jwt { get; set; }
        public string RefreshToken { get; set; }
    }
}
