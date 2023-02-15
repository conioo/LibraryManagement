#nullable disable

namespace Domain.Settings
{
    public class JwtSettings
    {
        public string IssuerSigningKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public double ExpireInMinutes { get; set; }
        public double RefreshTokenExpireInDays { get; set; }
    }
}
