using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Sieve.Services;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ApplicationTests")]

namespace Application.Services
{
    internal class ItemService : CommonService<Item, ItemRequest, ItemResponse>, IItemService
    {
        public ItemService(IUnitOfWork unitOfWork, IMapper mapper, ISieveProcessor sieveProcessor) : base(unitOfWork, mapper, sieveProcessor)
        { }

    }
}
