using Domain.Entities;

namespace Application.Reactive.Interfaces
{
    public interface ICountingOfPenaltyCharges : IObserver<DateTimeOffset>
    {
        public bool ReturnOfItem(Rental rental);
        public void AddRental(Rental rental);
        public bool RenewalRental(string rentalId, DateOnly oldEndDate, DateOnly newEndDate);
    }
}
