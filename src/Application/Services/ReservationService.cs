using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces;
using Application.Reactive.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using Profile = Domain.Entities.Profile;

namespace Application.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReservationService> _logger;
        private readonly ReservationSettings _reservationSettings;
        private readonly RentalSettings _rentalSettings;
        private readonly IMapper _mapper;
        private readonly IEndOfReservation _endOfReservation;
        private readonly ICountingOfPenaltyCharges _countingOfPenaltyCharge;
        private readonly IUserResolverService _userResolverService;

        public ReservationService(IUnitOfWork unitOfWork, IOptions<ReservationSettings> reservationOptions, IOptions<RentalSettings> rentalOptions, IMapper mapper, IEndOfReservation endOfReservation, ICountingOfPenaltyCharges countingOfPenaltyCharges, IUserResolverService userResolverService, ILogger<ReservationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _reservationSettings = reservationOptions.Value;
            _rentalSettings = rentalOptions.Value;
            _mapper = mapper;
            _endOfReservation = endOfReservation;
            _countingOfPenaltyCharge = countingOfPenaltyCharges;
            _userResolverService = userResolverService;
        }
        public async Task<ReservationResponse> AddReservationAsync(ReservationRequest request)
        {
            var profileLibraryCardNumber = _userResolverService.GetProfileCardNumber;

            if (profileLibraryCardNumber is null)
            {
                throw new BadRequestException();
            }

            var profileInfo = await _unitOfWork.Set<Profile>()
               .Where(profile => profile.LibraryCardNumber == profileLibraryCardNumber)
               .Select(profile => new { profile.IsActive, profile.UserId, CurrentReservationCount = profile.CurrentReservations.Count })
               .FirstOrDefaultAsync();

            if (profileInfo is null)
            {
                throw new NotFoundException("profile not found");
            }

            if (profileInfo.IsActive is false)
            {
                throw new BadRequestException("profile is deactivate");
            }

            if (profileInfo.CurrentReservationCount >= _reservationSettings.MaxReservationForUser)
            {
                throw new BadRequestException("user has a maximum number of reservations");
            }

            var copyInfo = await _unitOfWork.Set<Copy>()
                .Where(copy => copy.InventoryNumber == request.CopyInventoryNumber)
                .Select(copy => new { copy.IsAvailable })
                .FirstOrDefaultAsync();

            if (copyInfo is null)
            {
                throw new NotFoundException("copy not found");
            }

            if (copyInfo.IsAvailable is false)
            {
                throw new BadRequestException("copy doesn't available");
            }

            var referenceToProfile = new Profile() { LibraryCardNumber = profileLibraryCardNumber };
            var referenceToCopy = new Copy() { InventoryNumber = request.CopyInventoryNumber, IsAvailable = false };

            _unitOfWork.Set<Profile>().Attach(referenceToProfile);
            _unitOfWork.Set<Copy>().Attach(referenceToCopy);

            var newReservation = new Reservation()
            {
                BeginDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(_reservationSettings.TimeInDays)),
                Profile = referenceToProfile,
                Copy = referenceToCopy
            };

            _unitOfWork.Set<Copy>().Entry(referenceToCopy).Property(copy => copy.IsAvailable).IsModified = true;

            await _unitOfWork.Set<Reservation>().AddAsync(newReservation);

            await _unitOfWork.SaveChangesAsync();

            _endOfReservation.AddReservation(newReservation);

            _logger.LogInformation($"User: {profileInfo.UserId} reserved copy: {request.CopyInventoryNumber}");

            return _mapper.Map<ReservationResponse>(newReservation);
        }
        public async Task AddReservationsAsync(ICollection<ReservationRequest> requests)
        {
            var profileLibraryCardNumber = _userResolverService.GetProfileCardNumber;

            if (profileLibraryCardNumber is null)
            {
                throw new BadRequestException();
            }

            var profileInfo = await _unitOfWork.Set<Profile>()
              .Where(profile => profile.LibraryCardNumber == profileLibraryCardNumber)
              .Select(profile => new { profile.IsActive, profile.UserId, CurrentReservationCount = profile.CurrentReservations.Count })
              .FirstOrDefaultAsync();

            if (profileInfo is null)
            {
                throw new NotFoundException("profile not found");
            }

            if (profileInfo.IsActive is false)
            {
                throw new BadRequestException("profile is deactivate");
            }

            if (profileInfo.CurrentReservationCount + requests.Count > _reservationSettings.MaxReservationForUser)
            {
                throw new BadRequestException("it isn't possible to add so many reservations");
            }

            var referenceToProfile = new Profile() { LibraryCardNumber = profileLibraryCardNumber };
            _unitOfWork.Set<Profile>().Attach(referenceToProfile);

            var newReservations = new List<Reservation>();

            foreach (var request in requests)
            {
                var copyInfo = await _unitOfWork.Set<Copy>()
                    .Where(copy => copy.InventoryNumber == request.CopyInventoryNumber)
                    .Select(copy => new { copy.IsAvailable })
                    .FirstOrDefaultAsync();

                if (copyInfo is null)
                {
                    throw new NotFoundException($"copy {request.CopyInventoryNumber} not found");
                }

                if (copyInfo.IsAvailable is false)
                {
                    throw new BadRequestException($"copy {request.CopyInventoryNumber} doesn't available");
                }

                var referenceToCopy = new Copy() { InventoryNumber = request.CopyInventoryNumber, IsAvailable = false };
                _unitOfWork.Set<Copy>().Attach(referenceToCopy);

                newReservations.Add(new Reservation()
                {
                    BeginDate = DateOnly.FromDateTime(DateTime.Now),
                    EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(_reservationSettings.TimeInDays)),
                    Profile = referenceToProfile,
                    Copy = referenceToCopy
                });

                _unitOfWork.Set<Copy>().Entry(referenceToCopy).Property(copy => copy.IsAvailable).IsModified = true;
            }

            foreach (var reservation in newReservations)
            {
                _endOfReservation.AddReservation(reservation);

                _logger.LogInformation($"User: {profileInfo.UserId} reserved copy: {reservation.Copy.InventoryNumber}");
            }

            _unitOfWork.Set<Reservation>().AddRange(newReservations);

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<ReservationResponse> GetReservationById(string id)
        {
            var response = await _unitOfWork.Set<Reservation>()
              .AsNoTracking()
              .Where(reservation => reservation.Id == id)
              .ProjectTo<ReservationResponse>(_mapper.ConfigurationProvider)
              .FirstOrDefaultAsync();

            if (response is null)
            {
                throw new NotFoundException();
            }

            var profileLibraryCardNumber = _userResolverService.GetProfileCardNumber;

            if (response.ProfileLibraryCardNumber != profileLibraryCardNumber)
            {
                throw new AuthorizationException();
            }

            return response;
        }
        public async Task RemoveReservationByIdAsync(string id)
        {
            var reservationProfileCardNumber = await _unitOfWork.Set<Reservation>()
                .Where(reservation => reservation.Id == id)
                .Select(reservation => reservation.Profile.LibraryCardNumber)
                .FirstOrDefaultAsync();

            if (reservationProfileCardNumber is null)
            {
                throw new NotFoundException();
            }

            var profileLibraryCardNumber = _userResolverService.GetProfileCardNumber;

            if (reservationProfileCardNumber != profileLibraryCardNumber)
            {
                throw new AuthorizationException();
            }

            _unitOfWork.Set<Reservation>().Remove(new Reservation() { Id = id });
            await _unitOfWork.SaveChangesAsync();

            _endOfReservation.RemoveReservation(id);

            _logger.LogWarning("Removed reservation: {1}", id);
        }
        public async Task<RentalResponse> RentAsync(string id)
        {
            var reservation = await _unitOfWork.Set<Reservation>().Where(reservation => reservation.Id == id).Select(reservation => new Reservation() {
                Profile = new Profile() { LibraryCardNumber = reservation.Profile.LibraryCardNumber },
                Copy = new Copy() { InventoryNumber = reservation.Copy.InventoryNumber },
                EndDate = reservation.EndDate,
                BeginDate = reservation.BeginDate,
                Id = reservation.Id
            }).FirstOrDefaultAsync();

            if (reservation is null)
            {
                throw new NotFoundException();
            }

            var now = DateOnly.FromDateTime(DateTime.Now);

            var newRental = new Rental()
            {
                BeginDate = now,
                EndDate = now.AddDays(_rentalSettings.TimeInDays),
                Profile = reservation.Profile,
                Copy = reservation.Copy
            };

            _unitOfWork.Set<Profile>().Attach(reservation.Profile);
            _unitOfWork.Set<Copy>().Attach(reservation.Copy);

            _unitOfWork.Set<Reservation>().Remove(reservation);
            _unitOfWork.Set<Rental>().Add(newRental);

            await _unitOfWork.SaveChangesAsync();

            _countingOfPenaltyCharge.AddRental(newRental);
            _endOfReservation.RemoveReservation(reservation);

            return _mapper.Map<RentalResponse>(newRental);
        }
    }
}