﻿using Application.Dtos;
using Application.Interfaces;
using Application.Reactive.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Reactive.Observers
{
    internal class StartingCountingOfPenaltyCharges : IStartingCountingOfPenaltyCharges
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICountingOfPenaltyCharges _countingOfPernaltyCharges;
        private readonly ILogger<StartingCountingOfPenaltyCharges> _logger;
        private readonly RentalSettings _rentalOptions;
        private readonly IEmailService _emailService;
        private IDictionary<DateOnly, List<string>> _rentalIds = new Dictionary<DateOnly, List<string>>();

        public StartingCountingOfPenaltyCharges(IServiceProvider serviceProvider, ICountingOfPenaltyCharges countingOfPenaltyCharges, ILogger<StartingCountingOfPenaltyCharges> logger, IOptions<RentalSettings> options, IEmailService emailService)
        {
            _serviceProvider = serviceProvider;
            _countingOfPernaltyCharges = countingOfPenaltyCharges;
            _logger = logger;
            _rentalOptions = options.Value;
            _emailService = emailService;
        }

        public void AddRental(Rental rental)
        {
            var endOfRentalDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(_rentalOptions.TimeInDays);

            if (_rentalIds.ContainsKey(endOfRentalDate))
            {
                _rentalIds[endOfRentalDate].Add(rental.Id);
            }
            else
            {
                _rentalIds.Add(endOfRentalDate, new List<string>() { rental.Id });
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
            var date = DateOnly.FromDateTime(value.UtcDateTime).AddDays(-1);

            if (!_rentalIds.ContainsKey(date))
            {
                return;
            }

            var rentalIds = _rentalIds[date];

            Rental? rental;
            
            int numberAdded = 0;

            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                foreach (var id in rentalIds)
                {
                    rental = unitOfWork.Set<Rental>().Find(id);

                    if (rental is null)
                    {
                        _logger.LogInformation($"Rental with id: {id} not found");
                        break;
                    }

                    if (rental.IsReturned is false)
                    {
                        rental = unitOfWork.Set<Rental>().Include(rental => rental.Profile).Single(rental => rental.Id == id);

                        _emailService.SendAsync(new Email(userService.GetEmail(rental.Profile.UserId).Result, "Starting counting of penalty charges", "please pay"));

                        _countingOfPernaltyCharges.AddExtendedRental(rental);
                        ++numberAdded;
                    }
                }
            }

            _logger.LogInformation($"Starting counting {numberAdded} penalty charges");

            _rentalIds.Remove(date);
        }
    }
}
