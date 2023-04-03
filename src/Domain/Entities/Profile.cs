#pragma warning disable CS8618

using Domain.Common;

namespace Domain.Entities
{
    public class Profile : AuditableEntity
    {
        public string LibraryCardNumber { get; set; }
        public string UserId { get; set; }

        public bool IsActive { get; set; } =false;

        public ICollection<Rental>? HistoryRentals { get; set; }
        public ICollection<Reservation>? HistoryReservations { get; set; }

    }
}

//user ma profil
// tworzyc
// aktywacja profilu -> pracownik
// user -> remove tez profil
// regula autoryzacji dla
// telefon
// adres
// anulacja rezerwacji
// bez usera testy
// kody zwrotu
// get niezweryfikowane profile
// mapowanie profili, wypozyczne, reserwacji

// interfejsy
// confirm phone