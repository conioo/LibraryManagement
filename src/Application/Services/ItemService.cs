using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappings.AutoMapperHelpers;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sieve.Services;
using static Sieve.Extensions.MethodInfoExtended;

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
            if (dto.Images is not null)
            {
                var fileNames = await _filesService.SaveFilesAsync(dto.Images);

                var newItem = _mapper.Map<Item>(dto);
                newItem.ImagePaths = fileNames;

                return await base.AddAsync(newItem);
            }
            else
            {
                return await base.AddAsync(dto);
            }
        }

        public Task AddImagesAsync(ICollection<IFormFile> images)
        {
            throw new NotImplementedException();
        }

        public async override Task AddRangeAsync(ICollection<ItemRequest> dtos)
        {
            var items = new List<Item>();

            foreach (var dto in dtos)
            {
                var newItem = _mapper.Map<Item>(dto);
                items.Add(newItem);

                if (dto.Images is not null)
                {
                    var fileNames = await _filesService.SaveFilesAsync(dto.Images);

                    newItem.ImagePaths = fileNames;
                }
            }

            await base.AddRangeAsync(items);
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

        public Task RemoveImagesAsync(ICollection<string> imagePaths)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(string id, UpdateItemRequest dto)
        {
            var existedEntity = await _unitOfWork.Set<Item>().FindAsync(id);

            if (existedEntity is null)
            {
                throw new NotFoundException();
            }

            var updatedEntity = _mapper.Map(dto, existedEntity);

            if (dto.ImagePathsToDelete is not null)
            {
                if (updatedEntity.ImagePaths is null)
                {
                    throw new BadRequestException("entity doesn't contain image path");
                }

                foreach (var imagePath in dto.ImagePathsToDelete)
                {
                    if (updatedEntity.ImagePaths.Remove(imagePath) is false)
                    {
                        throw new BadRequestException("entity doesn't contain image path");
                    }
                }

                _filesService.RemoveFiles(dto.ImagePathsToDelete);
            }

            if (dto.ImagesToCreate is not null)
            {
                var fileNames = await _filesService.SaveFilesAsync(dto.ImagesToCreate);

                ((List<string>)updatedEntity.ImagePaths).AddRange(fileNames);//czy to może być nullem?
            }
            _unitOfWork.Set<Item>().Update(updatedEntity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"{_userResolverService.GetUserName} updated the Item with id: {id}");
        }
    }
}