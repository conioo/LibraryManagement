#nullable disable

namespace Domain.Settings
{
    public class ApplicationSettings
    {
        public string CallbackUrlForForgottenPassword { get; set; }
        public string CallbackUrlForVerificationEmail { get; set; }
        public string RoutePrefix { get; set; }
        public string BaseAddress { get; set; }
    }
}
