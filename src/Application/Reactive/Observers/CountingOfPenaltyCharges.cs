using Application.Exceptions;
using Application.Reactive.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Reactive.Observers
{
    internal class CountingOfPenaltyCharges : ICountingOfPenaltyCharges
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CountingOfPenaltyCharges> _logger;
        private readonly RentalSettings _rentalOptions;

        private IList<Rental> _extendedRentals = new List<Rental>();
        public CountingOfPenaltyCharges(IServiceProvider serviceProvider, ILogger<CountingOfPenaltyCharges> logger, IOptions<RentalSettings> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _rentalOptions = options.Value;
        }

        public void AddExtendedRental(Rental rental)
        {
            _extendedRentals.Add(rental);
        }

        public void RemoveExtendedRental(Rental rental)
        {
            var result = _extendedRentals.Remove(rental);

            if (result is false)
            {
                throw new NotFoundException();
            }
        }

        public void OnCompleted()
        {
            _logger.LogInformation($"Completed {this.GetType().Name}");
        }

        public void OnError(Exception error)
        {
            _logger.LogError(error.Message);
        }

        public void OnNext(DateTimeOffset value)
        {
            foreach (var rental in _extendedRentals)
            {
                rental.PenaltyCharge += _rentalOptions.PenaltyChargePerDay;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var uniOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                uniOfWork.Set<Rental>().UpdateRange(_extendedRentals);
                uniOfWork.SaveChangesAsync().Wait();
            }
        }
    }
}