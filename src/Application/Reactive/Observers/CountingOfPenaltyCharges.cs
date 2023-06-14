using Application.Dtos;
using Application.Interfaces;
using Application.Reactive.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("WebAPITests")]

namespace Application.Reactive.Observers
{
    internal class CountingOfPenaltyCharges : ICountingOfPenaltyCharges
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CountingOfPenaltyCharges> _logger;
        private readonly RentalSettings _rentalOptions;
        private readonly IEmailService _emailService;
        private IDictionary<DateOnly, List<string>> _rentalsId = new Dictionary<DateOnly, List<string>>();
        private IList<Rental> _notReturnedOnTime = new List<Rental>();
        public CountingOfPenaltyCharges(IServiceProvider serviceProvider, ILogger<CountingOfPenaltyCharges> logger, IOptions<RentalSettings> options, IEmailService emailService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _rentalOptions = options.Value;
            _emailService = emailService;
        }

        public void AddRental(Rental rental)
        {
            AddRental(rental.Id, rental.EndDate);
        }
        private void AddRental(string rentalId, DateOnly endDate)
        {
            if (_rentalsId.ContainsKey(endDate))
            {
                _rentalsId[endDate].Add(rentalId);
            }
            else
            {
                _rentalsId.Add(endDate, new List<string>() { rentalId });
            }
        }
        protected virtual TService GetService<TService>(IServiceProvider provider) where TService : class
        {
            return provider.GetRequiredService<TService>();
        }

        protected virtual IServiceScope CreateScope(IServiceProvider serviceProvider)
        {
            return serviceProvider.CreateScope();
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
            var date = DateOnly.FromDateTime(value.DateTime);

            using (var scope = CreateScope(_serviceProvider))
            {
                var unitOfWork = GetService<IUnitOfWork>(scope.ServiceProvider);

                if (_rentalsId.ContainsKey(date))
                {
                    var rentalIds = _rentalsId[date];
                    Rental? rental;
                    int numberAdded = 0;

                    var userService = GetService<IUserService>(scope.ServiceProvider);

                    foreach (var id in rentalIds)
                    {
                        rental = unitOfWork.Set<Rental>().Include(rental => rental.Profile).FirstOrDefault(rental => rental.Id == id);

                        if (rental is null)
                        {
                            _logger.LogInformation($"Rental with id: {id} not found");
                            break;
                        }

                        _emailService.SendAsync(new Email(userService.GetEmail(rental.Profile.UserId).Result, "Starting counting of penalty charges", "please pay"));

                        rental.PenaltyCharge = 0;
                        _notReturnedOnTime.Add(rental);

                        ++numberAdded;
                    }

                    _logger.LogInformation($"Starting counting {numberAdded} penalty charges");

                    _rentalsId.Remove(date);
                }

                foreach (var overdueRental in _notReturnedOnTime)
                {
                    overdueRental.PenaltyCharge += _rentalOptions.PenaltyChargePerDay;
                }

                unitOfWork.Set<Rental>().UpdateRange(_notReturnedOnTime);
                unitOfWork.SaveChangesAsync().Wait();
            }
        }

        public bool RenewalRental(string rentalId, DateOnly oldEndDate, DateOnly newEndDate)
        {
            if(_rentalsId.ContainsKey(oldEndDate) is false)
            {
                return false;
            }

            var found = _rentalsId[oldEndDate].Remove(rentalId);

            if (found is false)
            {
                return false;
            }

            AddRental(rentalId, newEndDate);

            return true;
        }

        public bool ReturnOfItem(Rental rental)
        {
            var endDate = rental.EndDate;

            if(_rentalsId.ContainsKey(endDate) is true)
            {
                var removedId = _rentalsId[endDate].Remove(rental.Id);

                if (removedId is true)
                {
                    return true;
                }
            }

            var toBeDeleted = _notReturnedOnTime.FirstOrDefault(obj => obj.Id == rental.Id);

            if(toBeDeleted is null)
            {
                return false;
            }

            _notReturnedOnTime.Remove(toBeDeleted);

            return true;
        }
    }
}