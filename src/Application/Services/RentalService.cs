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
using Profile = Domain.Entities.Profile;


namespace Application.Services
{
    public class RentalService : IRentalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICountingOfPenaltyCharges _countingOfPenaltyCharges;
        private readonly RentalSettings _rentalSettings;
        private readonly ILogger<RentalService> _logger;

        public RentalService(IUnitOfWork unitOfWork, IMapper mapper, ICountingOfPenaltyCharges countingOfPenaltyCharges, IOptions<RentalSettings> options, ILogger<RentalService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _countingOfPenaltyCharges = countingOfPenaltyCharges;
            _rentalSettings = options.Value;
            _logger = logger;
        }
        public async Task<RentalResponse> AddRentalAsync(RentalRequest request, string profileLibraryCardNumber)
        {
            var profile = await _unitOfWork.Set<Profile>()
                .Include(profile => profile.CurrrentRentals)
                .Where(profile => profile.LibraryCardNumber == profileLibraryCardNumber)
                .FirstOrDefaultAsync();

            if (profile is null)
            {
                throw new NotFoundException("profile not found");
            }

            if (profile.IsActive is false)
            {
                throw new BadRequestException("profile is deactivate");
            }

            if (profile.CurrrentRentals is not null && profile.CurrrentRentals.Count >= _rentalSettings.MaxRentalsForUser)
            {
                throw new BadRequestException("user has a maximum number of rentals");
            }


            var copy = await _unitOfWork.Set<Copy>().FindAsync(request.CopyInventoryNumber);

            if (copy is null)
            {
                throw new NotFoundException("copy not found");
            }

            if (copy.IsAvailable is false)
            {
                throw new BadRequestException("copy doesn't available");
            }

            var newRental = new Rental()
            {
                BeginDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(_rentalSettings.TimeInDays)),
                Profile = profile,
                Copy = copy
            };

            await _unitOfWork.Set<Rental>().AddAsync(newRental);
            copy.IsAvailable = false;
            await _unitOfWork.SaveChangesAsync();

            _countingOfPenaltyCharges.AddRental(newRental);

            _logger.LogInformation($"User: {profile.UserId} rented copy: {copy.InventoryNumber}");

            return _mapper.Map<RentalResponse>(newRental);
        }
        public async Task AddRentalsAsync(ICollection<RentalRequest> requests, string profileLibraryCardNumber)
        {
            var profile = await _unitOfWork.Set<Profile>()
                .AsNoTracking()
                .Include(profile => profile.CurrrentRentals)
                .Where(profile => profile.LibraryCardNumber == profileLibraryCardNumber)
                .FirstOrDefaultAsync();

            if (profile is null)
            {
                throw new NotFoundException("profile not found");
            }

            if (profile.IsActive is false)
            {
                throw new BadRequestException("profile is deactivate");
            }

            if (profile.CurrrentRentals is not null && profile.CurrrentRentals.Count + requests.Count > _rentalSettings.MaxRentalsForUser)
            {
                throw new BadRequestException("it isn't possible to add so many rentals");
            }

            var newRentals = new List<Rental>();

            foreach (var request in requests)
            {
                var copy = await _unitOfWork.Set<Copy>().FindAsync(request.CopyInventoryNumber);

                if (copy is null)
                {
                    throw new NotFoundException($"copy {request.CopyInventoryNumber} not found");
                }

                if (copy.IsAvailable is false)
                {
                    throw new BadRequestException($"copy {copy.InventoryNumber} doesn't available");
                }

                newRentals.Add(new Rental()
                {
                    BeginDate = DateOnly.FromDateTime(DateTime.Now),
                    EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(_rentalSettings.TimeInDays)),
                    //Profile = profile,
                    Copy = copy
                });

                copy.IsAvailable = false;
            }

            foreach (var rental in newRentals)
            {
                profile.CurrrentRentals.Add(rental);
                _countingOfPenaltyCharges.AddRental(rental);

                _logger.LogInformation($"User: {profile.UserId} rented copy: {rental.Copy.InventoryNumber}");
            }

            _unitOfWork.Set<Rental>().AddRange(newRentals);
            _unitOfWork.Set<Profile>().Update(profile);

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<RentalResponse> GetRentalById(string id)
        {
            var response = await _unitOfWork.Set<Rental>()
                .AsNoTracking()
                .Where(rental => rental.Id == id)
                .ProjectTo<RentalResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (response is null)
            {
                throw new NotFoundException();
            }

            return response;
        }
        public async Task RemoveRentalByIdAsync(string id)
        {
            var rental = await _unitOfWork.Set<Rental>().FindAsync(id);

            if (rental is null)
            {
                throw new NotFoundException();
            }

            _unitOfWork.Set<Rental>().Remove(rental);

            _countingOfPenaltyCharges.ReturnOfItem(rental);

            _logger.LogWarning($"Removed rental: {rental.Id}");//user id

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task RenewalAsync(string id)
        {
            var rentalToRenew = await _unitOfWork.Set<Rental>().FindAsync(id);

            if (rentalToRenew is null)
            {
                throw new NotFoundException();
            }

            if (rentalToRenew.NumberOfRenewals >= _rentalSettings.MaxNumberOfRenewals)
            {
                throw new BadRequestException("Maximum number of renewal has already been used");
            }

            _countingOfPenaltyCharges.RenewalRental(id, rentalToRenew.EndDate);

            rentalToRenew.EndDate = rentalToRenew.EndDate.AddDays(_rentalSettings.TimeInDays);
            rentalToRenew.NumberOfRenewals++;

            _unitOfWork.Set<Rental>().Update(rentalToRenew);
            _logger.LogInformation($"renewaled rental: {rentalToRenew.Id} ");

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task ReturnAsync(string id)
        {
            var rental = await _unitOfWork.Set<Rental>()
                .Include(rental => rental.Profile)
                .ThenInclude(profile => profile.ProfileHistory)
                .ThenInclude(profileHistory => profileHistory.ArchivalRentals)
                .Include(rental => rental.Copy)
                .ThenInclude(copy => copy.CopyHistory)
                .ThenInclude(copyHistory => copyHistory.ArchivalRentals)
                .Where(rental => rental.Id == id)
                .FirstOrDefaultAsync();

            if (rental is null)
            {
                throw new NotFoundException();
            }

            var archivalRental = GetArchivalRental(rental);

            await _unitOfWork.Set<ArchivalRental>().AddAsync(archivalRental);

            rental.Copy.CopyHistory.ArchivalRentals.Add(archivalRental);
            rental.Copy.IsAvailable = true;

            rental.Profile.ProfileHistory.ArchivalRentals.Add(archivalRental);

            _unitOfWork.Set<Rental>().Remove(rental);

            await _unitOfWork.SaveChangesAsync();

            _countingOfPenaltyCharges.ReturnOfItem(rental);

            _logger.LogInformation($"Returned rental: {rental.Id}");
        }

        public async Task ReturnsAsync(IEnumerable<string> ids)
        {
            if (ids.Distinct().Count() != ids.Count())
            {
                throw new BadRequestException("Id repetition detected");
            }


            var validRentals = new List<Rental>();

            foreach (var id in ids)
            {
                var rental = await _unitOfWork.Set<Rental>()
                  .Include(rental => rental.Profile)
                  .ThenInclude(profile => profile.ProfileHistory)
                  .ThenInclude(profileHistory => profileHistory.ArchivalRentals)
                  .Include(rental => rental.Copy)
                  .ThenInclude(copy => copy.CopyHistory)
                  .ThenInclude(copyHistory => copyHistory.ArchivalRentals)
                  .Where(rental => rental.Id == id)
                  .FirstOrDefaultAsync();

                if (rental is null)
                {
                    throw new NotFoundException($"rental with id: {id} not found");
                }

                var archivalRental = GetArchivalRental(rental);

                await _unitOfWork.Set<ArchivalRental>().AddAsync(archivalRental);

                rental.Copy.CopyHistory.ArchivalRentals.Add(archivalRental);
                rental.Copy.IsAvailable = true;

                rental.Profile.ProfileHistory.ArchivalRentals.Add(archivalRental);

                validRentals.Add(rental);
                _unitOfWork.Set<Rental>().Remove(rental);
            }

            await _unitOfWork.SaveChangesAsync();

            foreach (var rental in validRentals)
            {
                _countingOfPenaltyCharges.ReturnOfItem(rental);

                _logger.LogInformation($"Returned rental: {rental.Id}");
            }
        }

        private ArchivalRental GetArchivalRental(Rental rental)
        {
            var archivalRental = _mapper.Map<ArchivalRental>(rental);
            archivalRental.ReturnedDate = DateOnly.FromDateTime(DateTime.Now);

            return archivalRental;
        }
    }
}