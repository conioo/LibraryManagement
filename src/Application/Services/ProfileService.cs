using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Dtos.Response.Archive;
using Application.Exceptions;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Profile = Domain.Entities.Profile;

namespace Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IUserResolverService _userResolverService;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper, IUserResolverService userResolverService, ILogger<ProfileService> logger)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _mapper = mapper;
            _userResolverService = userResolverService;
            _logger = logger;
        }

        public async Task<ProfileResponse> CreateProfileAsync(ProfileRequest dto)
        {
            var userId = _userResolverService.GetUserId;

            if (userId is null)
            {
                throw new ApiException("user id cannot be downloaded");
            }

            var newProfil = new Profile();

            newProfil.UserId = userId;
            newProfil.ProfileHistory = new ProfileHistory();

            await _unitOfWork.Set<Profile>().AddAsync(newProfil);
            await _userService.BindProfil(userId, newProfil.LibraryCardNumber, dto);

            await _unitOfWork.SaveChangesAsync();

            var response = _mapper.Map<ProfileResponse>(newProfil);

            _logger.LogInformation($"Created Profile for user {userId}");

            return response;
        }

        public async Task ActivationProfileAsync(string cardNumber)
        {
            var profile = await _unitOfWork.Set<Profile>().FindAsync(cardNumber);

            if (profile is null)
            {
                throw new NotFoundException();
            }

            if (profile.IsActive is true)
            {
                throw new BadRequestException("Profile is already active");
            }

            profile.IsActive = true;

            _unitOfWork.Set<Profile>().Update(profile);

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"Activated profile for card number {cardNumber} ");
        }

        public async Task DeactivationProfileAsync(string cardNumber)
        {
            var profile = await _unitOfWork.Set<Profile>().FindAsync(cardNumber);

            if (profile is null)
            {
                throw new NotFoundException();
            }

            if (profile.IsActive is false)
            {
                throw new BadRequestException("Profile is already deactive");
            }

            profile.IsActive = false;

            _unitOfWork.Set<Profile>().Update(profile);

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"Deactivated profile for card number {cardNumber} ");
        }

        public async Task<ProfileResponse> GetProfileAsync()
        {
            var cardNumber = _userResolverService.GetProfileCardNumber;

            if (cardNumber is null || cardNumber == String.Empty)
            {
                throw new NotFoundException("user doesn't have a profile");
            }

            return await GetProfileByCardNumberAsync(cardNumber);
        }
        public async Task<ProfileResponse> GetProfileWithHistoryAsync()
        {
            var cardNumber = _userResolverService.GetProfileCardNumber;

            if (cardNumber is null || cardNumber == String.Empty)
            {
                throw new NotFoundException("user doesn't have a profile");
            }

            return await GetProfileWithHistoryByCardNumberAsync(cardNumber);
        }
        public async Task<ProfileResponse> GetProfileByCardNumberAsync(string cardNumber)
        {
            var profile = await _unitOfWork.Set<Profile>()
               .AsNoTracking()
               .Where(profile => profile.LibraryCardNumber == cardNumber)
               .ProjectTo<ProfileResponse>(_mapper.ConfigurationProvider)
               .FirstOrDefaultAsync();

            if (profile is null)
            {
                throw new NotFoundException();
            }

            var response = _mapper.Map<ProfileResponse>(profile);

            return response;
        }

        public async Task<ProfileResponse> GetProfileWithHistoryByCardNumberAsync(string cardNumber)
        {
            var profile = await _unitOfWork.Set<Profile>()
               .AsNoTracking()
               .Where(profile => profile.LibraryCardNumber == cardNumber)
               .ProjectTo<ProfileResponse>(_mapper.ConfigurationProvider, response => response.ProfileHistory)
               .FirstOrDefaultAsync();

            if (profile is null)
            {
                throw new NotFoundException();
            }

            var response = _mapper.Map<ProfileResponse>(profile);

            return response;
        }

        public async Task<ProfileHistoryResponse> GetProfileHistoryByCardNumberAsync(string cardNumber)
        {
            var response = await _unitOfWork.Set<Profile>()
               .AsNoTracking()
               .Where(profile => profile.LibraryCardNumber == cardNumber)
               .Select(profile => profile.ProfileHistory)
               .ProjectTo<ProfileHistoryResponse>(_mapper.ConfigurationProvider)
               .FirstOrDefaultAsync();

            if (response is null)
            {
                throw new NotFoundException();
            }

            return response;
            throw new NotImplementedException();
        }

        public async Task<ICollection<RentalResponse>> GetCurrentRentalsAsync(string cardNumber)
        {
            //var response = await _unitOfWork.Set<Profile>()
            // .AsNoTracking()
            // .Where(profile => profile.LibraryCardNumber == cardNumber)
            // .Select(profile => profile.CurrrentRentals)
            // .ProjectTo<ICollection<RentalResponse>>(_mapper.ConfigurationProvider)
            // .FirstOrDefaultAsync();

            var response = await _unitOfWork.Set<Profile>()
                       .AsNoTracking()
                       .Where(profile => profile.LibraryCardNumber == cardNumber)
                       .ProjectTo<ProfileResponse>(_mapper.ConfigurationProvider)
                       .Select(response => response.CurrrentRentals)
                       .FirstOrDefaultAsync();

            if (response is null)
            {
                throw new NotFoundException();
            }

            return response;
        }

        public async Task<ICollection<ReservationResponse>> GetCurrentReservationsAsync(string cardNumber)
        {
            var response = await _unitOfWork.Set<Profile>()
                       .AsNoTracking()
                       .Where(profile => profile.LibraryCardNumber == cardNumber)
                       .ProjectTo<ProfileResponse>(_mapper.ConfigurationProvider)
                       .Select(response => response.CurrrentReservations)
                       .FirstOrDefaultAsync();

            if (response is null)
            {
                throw new NotFoundException();
            }

            return response;
        }
    }
}