using Application.Dtos.Request;
using Application.Dtos.Response;

namespace Application.Interfaces
{
    public interface IItemService : ICommonService<ItemRequest, ItemResponse> 
    {
        public Task<IEnumerable<CopyResponse>> GetAllCopiesAsync(string id);
        public Task<IEnumerable<CopyResponse>> GetAvailableCopiesAsync(string id);
    }
}