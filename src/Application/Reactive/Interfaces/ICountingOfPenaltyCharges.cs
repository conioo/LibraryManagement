using Domain.Entities;

namespace Application.Reactive.Interfaces
{
    public interface ICountingOfPenaltyCharges : IObserver<DateTimeOffset>
    {
        public void RemoveOverdueRental(Rental rental);
        public void AddRental(Rental rental);
    }
}
