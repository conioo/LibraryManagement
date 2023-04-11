using Domain.Entities;

namespace Application.Reactive.Interfaces
{
    public interface ICountingOfPenaltyCharges : IObserver<DateTimeOffset>
    {
        public void AddExtendedRental(Rental rental);
        public void RemoveExtendedRental(Rental rental);
    }
}
