using Domain.Entities;
using Domain.Interfaces;

namespace Application.Reactive.Interfaces
{
    public interface IEndOfReservation : IObserver<DateTimeOffset>
    {
        public void AddReservation(Copy reservation);
    }
}
