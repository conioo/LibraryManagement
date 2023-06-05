using Domain.Entities;

namespace Application.Reactive.Interfaces
{
    public interface ICountingOfPenaltyCharges : IObserver<DateTimeOffset>
    {
        public void ReturnOfItem(Rental rental);
        public void AddRental(Rental rental);
        public bool RenewalRental(string rentalId, DateOnly endDate);
    }
}
