using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappings.AutoMapperHelpers;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sieve.Services;
using System.Security.Cryptography.X509Certificates;

namespace Application.Services
{
    internal class ItemService : CommonService<Item, ItemRequest, ItemResponse>, IItemService
    {
        public ItemService(IUnitOfWork unitOfWork, IMapper mapper, ISieveProcessor sieveProcessor, ILogger<ItemService> logger, IUserResolverService userResolverService) : base(unitOfWork, mapper, sieveProcessor, logger, userResolverService)
        { }

        public async Task<IEnumerable<CopyResponse>> GetAllCopiesAsync(string id)
        {
            var response = await _unitOfWork.Set<Item>()
                 .AsNoTracking()
                 .Where(item => item.Id == id)
                 .ProjectTo<CopiesHelper>(_mapper.ConfigurationProvider)
                 .Select(copiesHelper => copiesHelper.Copies)
                 .FirstOrDefaultAsync();

            if (response is null)
            {
                throw new NotFoundException();
            }

            return response;
        }

        public async Task<IEnumerable<CopyResponse>> GetAvailableCopiesAsync(string id)
        {
            var response = await _unitOfWork.Set<Item>()
               .AsNoTracking()
               .Where(item => item.Id == id)
               .ProjectTo<CopiesHelper>(_mapper.ConfigurationProvider)
               .Select(copiesHelper => copiesHelper.Copies.Where(copy => copy.IsAvailable == true))
               .FirstOrDefaultAsync();

            //var response = await _unitOfWork.Set<Item>()
            //       .AsNoTracking()
            //       .Where(item => item.Id == id)
            //       .Select(item => item.Copies.Where(copy => copy.IsAvailable == true))
            //       .ProjectTo<IEnumerable<CopyResponse>>(_mapper.ConfigurationProvider)
            //       .FirstOrDefaultAsync();

            if (response is null)
            {
                throw new NotFoundException();
            }

            return response;
        }
    }
}
