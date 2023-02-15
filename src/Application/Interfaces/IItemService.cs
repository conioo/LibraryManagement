using Application.Dtos.Request;
using Application.Dtos.Response;

namespace Application.Interfaces
{
    public interface IItemService : ICommonService<ItemRequest, ItemResponse> { }

}