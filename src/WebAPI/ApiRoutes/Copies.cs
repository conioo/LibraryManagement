using Microsoft.AspNetCore.Mvc;

namespace WebAPI.ApiRoutes
{
    public static class Copies
    {
        public const string Prefix = "copies";

        public const string GetHistoryByInventoryNumber = "history";
        public const string GetCopyById = "";
        public const string GetCurrentRental = "current-rental";
        public const string GetCurrentReservation = "current-reservation";

        public const string IsAvailable = "is-available";

        public const string AddCopies = "add";
        public const string RemoveCopy = "remove";
        public const string RemoveCopies = "remove-range";
    }
}
