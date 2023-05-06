using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Dtos.Response.Archive;

namespace Application.Interfaces
{
    public interface ICopyService : ICommonService<CopyRequest, CopyResponse>
    {
        public Task<CopyHistoryResponse> GetCopyHistoryAsync(string inventoryNumber);
        public Task<bool> IsAvailableAsync(string inventoryNumber);
        public Task<RentalResponse?> GetCurrentRentalAsync(string inventoryNumber);
        public Task<ReservationResponse?> GetCurrentReservationAsync(string inventoryNumber);
    }
}
