using Domain.Entities;

namespace WebAPI.ApiRoutes
{
    public class Profiles
    {
        public const string Prefix = "profiles";

        public const string CreateProfile = "create";
        public const string ActivationProfile = "activation";
        public const string DeactivationProfile = "deactivation";

        public const string GetProfileWithHistoryByCardNumber = "with-history";
        public const string GetProfile = "";
        public const string GetProfileWithHistory = "history";
        public const string GetProfileByCardNumber = "by-card/";
        public const string GetHistoryByCardNumber = "by-card/history";
        public const string GetCurrentRentals = "by-card/current-rentals";
        public const string GetCurrentReservations = "by-card/current-reservations";
    }
}
