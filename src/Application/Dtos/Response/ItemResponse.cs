#pragma warning disable CS8618
using Application.Dtos.Request;

namespace Application.Dtos.Response
{
    public class ItemResponse : ItemRequest
    {
        public string Id { get; set; }
    }
}