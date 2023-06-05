using Application.Dtos.Request;
using Application.Dtos.Response;

namespace Application.Interfaces
{
    public interface IRentalService
    {
        Task<RentalResponse> GetRentalById(string id);
        Task<RentalResponse> AddRentalAsync(RentalRequest request, string profileLibraryCardNumber);
        Task AddRentalsAsync(ICollection<RentalRequest> requests, string profileLibraryCardNumber);
        Task RemoveRentalByIdAsync(string id);
        Task RenewalAsync(string id);
        Task ReturnAsync(string id);
        Task ReturnsAsync(IEnumerable<string> ids);
    }
}
