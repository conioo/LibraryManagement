#pragma warning disable CS8618

namespace Application.Dtos.Identity.Request
{
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
