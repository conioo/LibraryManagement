using Domain.Entities;
using Domain.Interfaces;

namespace Application.Reactive.Interfaces
{
    public interface IStartingCountingOfPenaltyCharges : IObserver<DateTimeOffset>
    {
        public void AddRental(Rental rental);
    }
}
