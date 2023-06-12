using Application.Dtos.Request;
using Application.Dtos.Response;

namespace Application.Interfaces
{
    public interface IRentalService
    {
        Task<RentalResponse> AddRentalAsync(RentalRequest request, string profileLibraryCardNumber);
        Task AddRentalsAsync(ICollection<RentalRequest> requests, string profileLibraryCardNumber);
        Task<RentalResponse> GetRentalById(string id);
        Task PayThePenaltyAsync(string id);
        Task RemoveRentalByIdAsync(string id);
        Task RenewalAsync(string id);
        Task<bool> ReturnAsync(string id);
        Task<string> ReturnsAsync(IEnumerable<string> ids);
    }
}
