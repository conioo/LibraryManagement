using Application.Reactive.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Reactive.Observers
{
    internal class EndOfReservation : IEndOfReservation
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ReservationSettings _reservationOptions;
        private readonly ILogger<EndOfReservation> _logger;

        private IDictionary<DateOnly, List<Copy>> _copies = new Dictionary<DateOnly, List<Copy>>();//end reservation

        public EndOfReservation(IServiceProvider serviceProvider,/* IOptions<ReservationSettings> options,*/ ILogger<EndOfReservation> logger, IOptions<RentalSettings> ren)
        {
            _serviceProvider = serviceProvider;
            //_reservationOptions = options.Value;
            _logger = logger;
        }

        public void AddReservation(Copy copy)
        {
            var endOfReservationDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(_reservationOptions.TimeInDays);

            if (_copies.ContainsKey(endOfReservationDate))
            {
                _copies[endOfReservationDate].Add(copy);
            }
            else
            {
                _copies.Add(endOfReservationDate, new List<Copy>() { copy });
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
            var date = DateOnly.FromDateTime(DateTime.Now).AddDays(-1);

            if (!_copies.ContainsKey(date))
            {
                return;
            }

            var copies = _copies[date];

            int numberFailedReservation = 0;

            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                foreach (var copy in copies)
                {
                    Copy copyModel = unitOfWork.Set<Copy>().AsNoTracking().Include(model => model.CurrentReservation).Single(model => model.InventoryNumber == copy.InventoryNumber);

                    var reservation = copyModel.CurrentReservation;

                    if (reservation is null)
                    {
                        _logger.LogInformation($"The last reservation for copy: {copy.InventoryNumber} is missing");
                        break;
                    }

                    //if (reservation.Received is false)

                    ++numberFailedReservation;

                    copy.IsAvailable = true;

                }

                unitOfWork.Set<Copy>().UpdateRange(copies);
                unitOfWork.SaveChangesAsync();
            }

            _logger.LogInformation($"Failed {numberFailedReservation} reservations");
            _copies.Remove(date);
        }
    }
}