using Domain.Entities;
using Domain.Interfaces;

namespace Application.Reactive.Interfaces
{
    public interface IEndOfReservation : IObserver<DateTimeOffset>
    {
        public void AddReservation(Reservation reservation);
        public void AddReservation(string reservationId);
        public void RemoveReservation(Reservation reservation);
        public void RemoveReservation(string id);
    }
}