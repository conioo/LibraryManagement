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
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Profile = Domain.Entities.Profile;


namespace Application.Services
{
    public class RentalService : IRentalService
    {
        private readonly IDistributedCache _cache;
        private readonly ICountingOfPenaltyCharges _countingOfPenaltyCharges;
        private readonly ILogger<RentalService> _logger;
        private readonly IMapper _mapper;
        private readonly RentalSettings _rentalSettings;
        private readonly IUnitOfWork _unitOfWork;

        public RentalService(IUnitOfWork unitOfWork, IMapper mapper, ICountingOfPenaltyCharges countingOfPenaltyCharges, IOptions<RentalSettings> options, ILogger<RentalService> logger, IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _countingOfPenaltyCharges = countingOfPenaltyCharges;
            _rentalSettings = options.Value;
            _logger = logger;
            _cache = cache;
        }

        private ArchivalRental GetArchivalRental(Rental rental, string copyHistoryId, string profileHistoryId)
        {
            return GetArchivalRental(rental, new CopyHistory() { Id = copyHistoryId }, new ProfileHistory() { Id = profileHistoryId });
        }

        private ArchivalRental GetArchivalRental(Rental rental, CopyHistory referenceToCopyHistory, ProfileHistory referenceToProfileHistory)
        {
            _unitOfWork.Set<CopyHistory>().Attach(referenceToCopyHistory);
            _unitOfWork.Set<ProfileHistory>().Attach(referenceToProfileHistory);

            var archivalRental = _mapper.Map<ArchivalRental>(rental);
            archivalRental.ReturnedDate = DateOnly.FromDateTime(DateTime.Now);
            archivalRental.CopyHistory = referenceToCopyHistory;
            archivalRental.ProfileHistory = referenceToProfileHistory;

            return archivalRental;
        }
        private async Task SetCacheForPayment(string id)
        {
            await _cache.SetStringAsync(id, String.Empty, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
        }

        public async Task<RentalResponse> AddRentalAsync(RentalRequest request, string profileLibraryCardNumber)
        {
            var profileInfo = await _unitOfWork.Set<Profile>()
                .Where(profile => profile.LibraryCardNumber == profileLibraryCardNumber)
                .Select(profile => new { profile.IsActive, profile.UserId, CurrentRentalsCount = profile.CurrentRentals.Count })
                .FirstOrDefaultAsync();

            if (profileInfo is null)
            {
                throw new NotFoundException("profile not found");
            }

            if (profileInfo.IsActive is false)
            {
                throw new BadRequestException("profile is deactivate");
            }

            if (profileInfo.CurrentRentalsCount >= _rentalSettings.MaxRentalsForUser)
            {
                throw new BadRequestException("user has a maximum number of rentals");
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

            var newRental = new Rental()
            {
                BeginDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(_rentalSettings.TimeInDays)),
                Profile = referenceToProfile,
                Copy = referenceToCopy
            };

            //await _unitOfWork.Set<Copy>().Where(copy => copy.InventoryNumber == profileLibraryCardNumber).ExecuteUpdateAsync(s => s.SetProperty(copy => copy.IsAvailable, _ => false));

            _unitOfWork.Set<Copy>().Entry(referenceToCopy).Property(copy => copy.IsAvailable).IsModified = true;
            await _unitOfWork.Set<Rental>().AddAsync(newRental);

            await _unitOfWork.SaveChangesAsync();

            _countingOfPenaltyCharges.AddRental(newRental);

            _logger.LogInformation($"User: {profileInfo.UserId} rented copy: {request.CopyInventoryNumber}");

            return _mapper.Map<RentalResponse>(newRental);
        }
        public async Task AddRentalsAsync(ICollection<RentalRequest> requests, string profileLibraryCardNumber)
        {
            var profileInfo = await _unitOfWork.Set<Profile>()
                .Where(profile => profile.LibraryCardNumber == profileLibraryCardNumber)
                .Select(profile => new { profile.IsActive, profile.UserId, CurrentRentalsCount = profile.CurrentRentals.Count })
                .FirstOrDefaultAsync();

            if (profileInfo is null)
            {
                throw new NotFoundException("profile not found");
            }

            if (profileInfo.IsActive is false)
            {
                throw new BadRequestException("profile is deactivate");
            }

            if (profileInfo.CurrentRentalsCount + requests.Count > _rentalSettings.MaxRentalsForUser)
            {
                throw new BadRequestException("it isn't possible to add so many rentals");
            }

            var referenceToProfile = new Profile() { LibraryCardNumber = profileLibraryCardNumber };
            _unitOfWork.Set<Profile>().Attach(referenceToProfile);

            var newRentals = new List<Rental>();

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

                newRentals.Add(new Rental()
                {
                    BeginDate = DateOnly.FromDateTime(DateTime.Now),
                    EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(_rentalSettings.TimeInDays)),
                    Profile = referenceToProfile,
                    Copy = referenceToCopy
                });

                _unitOfWork.Set<Copy>().Entry(referenceToCopy).Property(copy => copy.IsAvailable).IsModified = true;
            }

            foreach (var rental in newRentals)
            {
                _countingOfPenaltyCharges.AddRental(rental);

                _logger.LogInformation($"User: {profileInfo.UserId} rented copy: {rental.Copy.InventoryNumber}");
            }

            _unitOfWork.Set<Rental>().AddRange(newRentals);

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
        public async Task PayThePenaltyAsync(string id)
        {
            if (await _cache.GetStringAsync(id) == null)
            {
                throw new BadRequestException();
            }

            bool successfulPayment = true;

            if (successfulPayment is false)
            {
                throw new ApiException();
            }

            var rentalInfo = await _unitOfWork.Set<Rental>()
                .Where(rental => rental.Id == id)
                .Select(rental => new { rental, rental.Copy.CopyHistoryId, rental.Profile.ProfileHistoryId, CopyInventoryNumber = rental.Copy.InventoryNumber })
                .FirstOrDefaultAsync();

            if (rentalInfo is null)
            {
                throw new NotFoundException();
            }

            var archivalRental = GetArchivalRental(rentalInfo.rental, rentalInfo.CopyHistoryId, rentalInfo.ProfileHistoryId);
            await _unitOfWork.Set<ArchivalRental>().AddAsync(archivalRental);

            var modifiedCopy = new Copy() { InventoryNumber = rentalInfo.CopyInventoryNumber, IsAvailable = true };
            _unitOfWork.Set<Copy>().Entry(modifiedCopy).Property(copy => copy.IsAvailable).IsModified = true;

            _unitOfWork.Set<Rental>().Remove(rentalInfo.rental);

            await _unitOfWork.SaveChangesAsync();

            _countingOfPenaltyCharges.ReturnOfItem(rentalInfo.rental);

            _logger.LogInformation($"Paid and returned rental: {rentalInfo.rental.Id}");
        }
        public async Task RemoveRentalByIdAsync(string id)
        {
            var rental = await _unitOfWork.Set<Rental>().FindAsync(id);

            if (rental is null)
            {
                throw new NotFoundException();
            }

            _unitOfWork.Set<Rental>().Remove(rental);
            await _unitOfWork.SaveChangesAsync();

            _countingOfPenaltyCharges.ReturnOfItem(rental);

            _logger.LogWarning("Removed rental: {rentalId}", rental.Id);
        }
        public async Task RenewalAsync(string id)
        {
            var rentalToRenewInfo = await _unitOfWork.Set<Rental>().Where(rental => rental.Id == id).Select(rental => new { rental.NumberOfRenewals, rental.EndDate }).FirstOrDefaultAsync();

            if (rentalToRenewInfo is null)
            {
                throw new NotFoundException();
            }

            if (rentalToRenewInfo.NumberOfRenewals >= _rentalSettings.MaxNumberOfRenewals)
            {
                throw new BadRequestException("Maximum number of renewal has already been used");
            }

            var newEndDate = rentalToRenewInfo.EndDate.AddDays(_rentalSettings.TimeInDays);

            _countingOfPenaltyCharges.RenewalRental(id, rentalToRenewInfo.EndDate, newEndDate);

            var modifiedRental = new Rental()
            {
                Id = id,
                EndDate = newEndDate,
                NumberOfRenewals = rentalToRenewInfo.NumberOfRenewals + 1
            };

            _unitOfWork.Set<Rental>().Entry(modifiedRental).Property(rental => rental.EndDate).IsModified = true;
            _unitOfWork.Set<Rental>().Entry(modifiedRental).Property(rental => rental.NumberOfRenewals).IsModified = true;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"renewaled rental: {id} ");
        }
        public async Task<bool> ReturnAsync(string id)
        {
            var rentalInfo = await _unitOfWork.Set<Rental>()
                .Where(rental => rental.Id == id)
                .Select(rental => new { rental, rental.Copy.CopyHistoryId, rental.Profile.ProfileHistoryId, CopyInventoryNumber = rental.Copy.InventoryNumber, rental.PenaltyCharge })
                .FirstOrDefaultAsync();

            if (rentalInfo is null)
            {
                throw new NotFoundException();
            }

            if (rentalInfo.PenaltyCharge is not null)
            {
                await SetCacheForPayment(id);

                return false;
            }

            var archivalRental = GetArchivalRental(rentalInfo.rental, rentalInfo.CopyHistoryId, rentalInfo.ProfileHistoryId);
            await _unitOfWork.Set<ArchivalRental>().AddAsync(archivalRental);

            var modifiedCopy = new Copy() { InventoryNumber = rentalInfo.CopyInventoryNumber, IsAvailable = true };
            _unitOfWork.Set<Copy>().Entry(modifiedCopy).Property(copy => copy.IsAvailable).IsModified = true;

            _unitOfWork.Set<Rental>().Remove(rentalInfo.rental);

            await _unitOfWork.SaveChangesAsync();

            _countingOfPenaltyCharges.ReturnOfItem(rentalInfo.rental);

            _logger.LogInformation($"Returned rental: {id}");

            return true;
        }
        public async Task<string> ReturnsAsync(IEnumerable<string> ids)
        {
            if (ids.Distinct().Count() != ids.Count())
            {
                throw new BadRequestException("Id repetition detected");
            }

            var validRentals = new List<Rental>();
            var idsToBePaid = new List<string>();
            var archivalRentalsToAdded = new List<ArchivalRental>();
            string result = string.Empty;
            var copyHistoriesAttached = new Dictionary<string, CopyHistory>();
            var profileHistoriesAttached = new Dictionary<string, ProfileHistory>();

            foreach (var id in ids)
            {
                var rentalInfo = await _unitOfWork.Set<Rental>()
                  .Where(rental => rental.Id == id)
                  .Select(rental => new { rental, rental.Copy.CopyHistoryId, rental.Profile.ProfileHistoryId, CopyInventoryNumber = rental.Copy.InventoryNumber, rental.PenaltyCharge })
                  .FirstOrDefaultAsync();

                if (rentalInfo is null)
                {
                    throw new NotFoundException($"rental with id: {id} not found");
                }

                if (rentalInfo.PenaltyCharge is not null)
                {
                    idsToBePaid.Add(id);
                    continue;
                }

                if (copyHistoriesAttached.TryGetValue(rentalInfo.CopyHistoryId, out var referenceToCopyHistory) is false)
                {
                    referenceToCopyHistory = new CopyHistory() { Id = rentalInfo.CopyHistoryId };
                    _unitOfWork.Set<CopyHistory>().Attach(referenceToCopyHistory);
                    copyHistoriesAttached.Add(rentalInfo.CopyHistoryId, referenceToCopyHistory);
                }

                if (profileHistoriesAttached.TryGetValue(rentalInfo.ProfileHistoryId, out var referenceToProfileHistory) is false)
                {
                    referenceToProfileHistory = new ProfileHistory() { Id = rentalInfo.ProfileHistoryId };
                    _unitOfWork.Set<ProfileHistory>().Attach(referenceToProfileHistory);
                    profileHistoriesAttached.Add(rentalInfo.ProfileHistoryId, referenceToProfileHistory);
                }

                var archivalRental = GetArchivalRental(rentalInfo.rental, referenceToCopyHistory, referenceToProfileHistory);

                archivalRentalsToAdded.Add(archivalRental);

                var modifiedCopy = new Copy() { InventoryNumber = rentalInfo.CopyInventoryNumber, IsAvailable = true };
                _unitOfWork.Set<Copy>().Entry(modifiedCopy).Property(copy => copy.IsAvailable).IsModified = true;

                validRentals.Add(rentalInfo.rental);
            }

            _unitOfWork.Set<Rental>().RemoveRange(validRentals);
            await _unitOfWork.Set<ArchivalRental>().AddRangeAsync(archivalRentalsToAdded);
            await _unitOfWork.SaveChangesAsync();

            foreach (var rental in validRentals)
            {
                _countingOfPenaltyCharges.ReturnOfItem(rental);

                _logger.LogInformation($"Returned rental: {rental.Id}");
            }

            foreach (var rentalId in idsToBePaid)
            {
                await SetCacheForPayment(rentalId);

                result += $"{rentalId} ";
            }

            if (result != string.Empty)
            {
                result = result.Remove(result.Length - 1, 1);
            }

            return result;
        }
    }
}