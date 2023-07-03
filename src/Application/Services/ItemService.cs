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

namespace Application.Services
{
    internal class ItemService : CommonService<Item, ItemRequest, ItemResponse>, IItemService
    {
        private readonly IFilesService _filesService;

        public ItemService(IUnitOfWork unitOfWork, IMapper mapper, ISieveProcessor sieveProcessor, ILogger<ItemService> logger, IUserResolverService userResolverService, IFilesService filesSevice) : base(unitOfWork, mapper, sieveProcessor, logger, userResolverService)
        {
            _filesService = filesSevice;
        }

        public async override Task<ItemResponse> AddAsync(ItemRequest dto)
        {
            if(dto.Images is not null)
            {
                var fileNames = await _filesService.SaveFilesAsync(dto.Images);
                var newItem = new Item() { ImagePaths = fileNames };

                return await base.AddAsync(newItem);
            }
            else
            {
                return await base.AddAsync(dto);
            }
        }
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
