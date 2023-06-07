using Domain.Interfaces;

namespace CommonContext.Extensions
{
    public static class IHistoryExtension
    {
        public static void Clear(this IHistory iHistory)
        {
            iHistory.ArchivalRentals.Clear();
            iHistory.ArchivalReservations.Clear();
        }
    }
}
