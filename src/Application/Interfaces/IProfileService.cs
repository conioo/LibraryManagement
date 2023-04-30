using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Dtos.Response.Archive;

namespace Application.Interfaces
{
    public interface IProfileService
    {
        public Task<ProfileResponse> CreateProfileAsync(ProfileRequest profile);
        public Task ActivationProfileAsync(string cardNumber);
        public Task DeactivationProfileAsync(string cardNumber);
        public Task<ProfileResponse> GetProfileAsync();
        public Task<ProfileResponse> GetProfileWithHistoryAsync();
        public Task<ProfileResponse> GetProfileByCardNumberAsync(string cardNumber);
        public Task<ProfileResponse> GetProfileWithHistoryByCardNumberAsync(string cardNumber);
        public Task<ProfileHistoryResponse> GetProfileHistoryByCardNumberAsync(string cardNumber);
        public Task<ICollection<RentalResponse>> GetCurrentRentalsAsync(string cardNumber);
        public Task<ICollection<ReservationResponse>> GetCurrentReservationsAsync(string cardNumber);
    }
}
