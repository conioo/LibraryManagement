using Application.Dtos.Request;
using Application.Dtos.Response;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    internal class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ItemRequest, Item>();
            CreateMap<ItemRequest, ItemResponse>();
            CreateMap<Item, ItemResponse>();
        }
    }
}
