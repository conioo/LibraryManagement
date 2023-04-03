using Application.Dtos.Request;
using Application.Dtos.Response;

namespace Application.Interfaces
{
    public interface IProfileService
    {
        public Task<ProfileResponse> CreateProfileAsync(ProfileRequest profile);
        public Task ActivationProfileAsync(string cardNumber);
        public Task<ProfileResponse> GetProfileByCardNumberAsync(string cardNumber);
        public Task<ProfileResponse> GetProfileWithHistoryByCardNumberAsync(string cardNumber);
        public Task<IEnumerable<RentalResponse>> GetRentalHistoryAsync(string cardNumber);
        public Task<IEnumerable<RentalResponse>> GetUnreturnedRentalsAsync(string cardNumber);
        public Task<IEnumerable<ReservationResponse>> GetReservationHistoryAsync(string cardNumber);
        public Task<IEnumerable<ReservationResponse>> GetUnreceivedReservationsAsync(string cardNumber);
    }
}
