#pragma warning disable CS8618 


namespace Application.Dtos.Identity.Request
{
    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}