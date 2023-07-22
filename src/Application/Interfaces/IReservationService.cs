using Application.Dtos.Request;
using Application.Dtos.Response;

namespace Application.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationResponse> AddReservationAsync(ReservationRequest requestr);
        Task AddReservationsAsync(ICollection<ReservationRequest> requests);
        Task<ReservationResponse> GetReservationById(string id);
        Task RemoveReservationByIdAsync(string id);
        Task<RentalResponse> RentAsync(string id);
    }
}