using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Profile = Domain.Entities.Profile;

namespace Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IUserResolverService _userResolverService;

        public ProfileService(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper,  IUserResolverService userResolverService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _mapper = mapper;
            _userResolverService = userResolverService;
        }

        public async Task<ProfileResponse> CreateProfileAsync(ProfileRequest dto)
        {
            var newProfil = new Profile();

            var userId = _userResolverService.GetUserId;

            if (userId is null)
            {
                throw new ApiException("user id cannot be downloaded");
            }

            newProfil.UserId = userId;

            var entityEntry = await _unitOfWork.Set<Profile>().AddAsync(newProfil);

            await _userService.BindProfil(userId, newProfil.LibraryCardNumber, dto);

            var response = _mapper.Map<ProfileResponse>(entityEntry.Entity);

            await _unitOfWork.SaveChangesAsync();

            return response;
        }

        public async Task ActivationProfileAsync(string cardNumber)
        {
            var profile = await _unitOfWork.Set<Profile>().FindAsync(cardNumber);

            if(profile is null)
            {
                throw new NotFoundException();
            }

            if(profile.IsActive is true)
            {
                throw new BadRequestException("Profile is already active");
            }

            profile.IsActive = true;

            _unitOfWork.Set<Profile>().Update(profile);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ProfileResponse> GetProfileWithHistoryByCardNumberAsync(string cardNumber)
        {
            //var profile = await _unitOfWork.Set<Profile>().AsNoTracking()
            //    .Include(profile => profile.HistoryRentals)
            //    .ThenInclude(rental => rental.Copy)
            //    .ThenInclude(copy => copy.Item)
            //    .Include(profile => profile.HistoryReservations)
            //    .ThenInclude(reservation => reservation.Copy)
            //    .ThenInclude(copy => copy.Item)
            //    .FirstAsync(profile => profile.LibraryCardNumber == cardNumber);

            //if (profile is null)
            //{
            //    throw new NotFoundException();
            //}

            //var response = _mapper.Map<ProfileResponse>(profile);

            //return response;
            return null;
        }

        public async Task<ProfileResponse> GetProfileByCardNumberAsync(string cardNumber)
        {
            var profile = await _unitOfWork.Set<Profile>().FindAsync(cardNumber);

            if (profile is null)
            {
                throw new NotFoundException();
            }

            var response = _mapper.Map<ProfileResponse>(profile);

            return response;
        }

        public async Task<IEnumerable<RentalResponse>> GetRentalHistoryAsync(string cardNumber)
        {
            //var profile = await _unitOfWork.Set<Profile>().AsNoTracking()
            //    .Include(profile => profile.HistoryRentals)
            //    .ThenInclude(rental => rental.Copy)
            //    .ThenInclude(copy => copy.Item)
            //    .FirstAsync(profile => profile.LibraryCardNumber == cardNumber);

            //if (profile is null)
            //{
            //    throw new NotFoundException();
            //}

            //var response = _mapper.Map<IEnumerable<RentalResponse>>(profile.HistoryRentals);

            //return response;
            return null;
        }

        public async Task<IEnumerable<ReservationResponse>> GetReservationHistoryAsync(string cardNumber)
        {
            //var profile = await _unitOfWork.Set<Profile>().AsNoTracking()
            //    .Include(profile => profile.HistoryReservations)
            //    .ThenInclude(reservation => reservation.Copy)
            //    .ThenInclude(copy => copy.Item)
            //    .FirstAsync(profile => profile.LibraryCardNumber == cardNumber);

            //if (profile is null)
            //{
            //    throw new NotFoundException();
            //}

            //var response = _mapper.Map<IEnumerable<ReservationResponse>>(profile.HistoryRentals);

            //return response;
            return null;
        }

        public async Task<IEnumerable<RentalResponse>> GetUnreturnedRentalsAsync(string cardNumber)
        {
            //var profile = await _unitOfWork.Set<Profile>().FindAsync(cardNumber);

            //if (profile is null)
            //{
            //    throw new NotFoundException();
            //}

            //var response = _mapper.Map<IEnumerable<RentalResponse>>(profile.HistoryRentals);

            //return response;
            return null;
        }

        public Task<IEnumerable<ReservationResponse>> GetUnreceivedReservationsAsync(string cardNumber)
        {
            throw new NotImplementedException();
        }
    }
}
