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

namespace Application.Reactive.Observers
{
    internal class EndOfReservation : IEndOfReservation
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ReservationSettings _reservationOptions;
        private readonly ILogger<EndOfReservation> _logger;
        private readonly IEmailService _emailService;
        private IDictionary<DateOnly, List<string>> _reservationsId = new Dictionary<DateOnly, List<string>>();

        public EndOfReservation(IServiceProvider serviceProvider, IOptions<ReservationSettings> options, IEmailService emailService, ILogger<EndOfReservation> logger)
        {
            _serviceProvider = serviceProvider;
            _reservationOptions = options.Value;
            _logger = logger;
            _emailService = emailService;
        }
        //wymusic wczesniejesze
        //testy dla reactive endofreserwation jak counting 
        //remove dokonczyc

        //pobrac -> do historii
        private ArchivalReservation? GerArchivalReservation(IUnitOfWork unitOfWork, string reservationId, out string? userId)
        {
            var reservationInfo = unitOfWork.Set<Reservation>()
                       .Where(reservation => reservation.Id == reservationId).Select(reservation => new
                       {
                           reservation,
                           reservation.Profile.UserId,
                           reservation.Profile.ProfileHistoryId,
                           reservation.Copy.CopyHistoryId,
                           CopyInventoryNumber = reservation.Copy.InventoryNumber
                       }).FirstOrDefault();

            if (reservationInfo is null)
            {
                _logger.LogInformation($"Reservation with id: {reservationId} not found");
                userId = null;
                return null;
            }

            var referenceToCopy = new Copy() { InventoryNumber = reservationInfo.CopyInventoryNumber, IsAvailable = true };

            unitOfWork.Set<Copy>().Attach(referenceToCopy);
            unitOfWork.Set<Copy>().Entry(referenceToCopy).Property(copy => copy.IsAvailable).IsModified = true;

            var referenceToProfileHistory = new ProfileHistory() { Id = reservationInfo.ProfileHistoryId };
            var referenceToCopyHistory = new CopyHistory() { Id = reservationInfo.CopyHistoryId };

            unitOfWork.Set<ProfileHistory>().Attach(referenceToProfileHistory);
            unitOfWork.Set<CopyHistory>().Attach(referenceToCopyHistory);

            userId = reservationInfo.UserId;

            return (new ArchivalReservation()
            {
                BeginDate = reservationInfo.reservation.BeginDate,
                EndDate = reservationInfo.reservation.EndDate,
                ProfileHistory = referenceToProfileHistory,
                CopyHistory = referenceToCopyHistory
            });
        }
        protected virtual TService GetService<TService>(IServiceProvider provider) where TService : class
        {
            return provider.GetRequiredService<TService>();
        }
        protected virtual IServiceScope CreateScope(IServiceProvider serviceProvider)
        {
            return serviceProvider.CreateScope();
        }

        public void AddReservation(string reservationId)
        {
            var endOfReservationDate = DateOnly.FromDateTime(DateTime.Now).AddDays(_reservationOptions.TimeInDays);

            if (_reservationsId.ContainsKey(endOfReservationDate))
            {
                _reservationsId[endOfReservationDate].Add(reservationId);
            }
            else
            {
                _reservationsId.Add(endOfReservationDate, new List<string>() { reservationId });
            }

        }
        public void AddReservation(Reservation reservation)
        {
            AddReservation(reservation.Id);
        }
        public void RemoveReservation(string id)
        {
            //remeove i dohistori
            //_reservationsId

        }

        public void RemoveReservation(Reservation reservation)
        {
            var endDate = reservation.EndDate;

            if (_reservationsId.ContainsKey(endDate) is false)
            {
                return;
            }

            _reservationsId[endDate].Remove(reservation.Id);

            // do historii dodac

            using (var scope = CreateScope(_serviceProvider))
            {
                var unitOfWork = GetService<IUnitOfWork>(scope.ServiceProvider);

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
            var date = DateOnly.FromDateTime(value.DateTime);

            if (_reservationsId.ContainsKey(date) is false)
            {
                return;
            }

            using (var scope = CreateScope(_serviceProvider))
            {
                var unitOfWork = GetService<IUnitOfWork>(scope.ServiceProvider);

                var reservationIds = _reservationsId[date];
                int numberEnded = 0;
                var archivalReservations = new List<ArchivalReservation>();
                var userService = GetService<IUserService>(scope.ServiceProvider);

                foreach (var id in reservationIds)
                {
                    var archivalReservation = GerArchivalReservation(unitOfWork, id, out string? userId);

                    if (userId is not null)
                    {
                        _emailService.SendAsync(new Email(userService.GetEmail(userId).Result, "The reservation period has expired", "Reservation period has ended"));
                    }

                    if (archivalReservation is not null)
                    {
                        archivalReservations.Add(archivalReservation);
                    }

                    ++numberEnded;
                }

                _logger.LogInformation("{1} reservations has expired", numberEnded);

                unitOfWork.Set<ArchivalReservation>().AddRange(archivalReservations);
                unitOfWork.SaveChanges();

                _reservationsId.Remove(date);
            }
        }
    }
}