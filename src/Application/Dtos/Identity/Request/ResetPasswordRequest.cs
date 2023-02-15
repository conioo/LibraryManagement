#pragma warning disable CS8618

namespace Application.Dtos.Identity.Request
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string PasswordResetToken { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
