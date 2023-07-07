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

namespace Application.Services
{
    internal class ItemService : CommonService<Item, ItemRequest, ItemResponse>, IItemService
    {
        private readonly IFilesService _filesService;

        private void RemoveImagesFromItem(Item entity, ICollection<string> imagePaths)
        {
            if (entity.ImagePaths is null)
            {
                throw new BadRequestException("entity doesn't contain image path");
            }

            foreach (var imagePath in imagePaths)
            {
                if (entity.ImagePaths.Remove(imagePath) is false)
                {
                    throw new BadRequestException("entity doesn't contain image path");
                }
            }

            _filesService.RemoveFiles(imagePaths);
        }

        private async Task AddImagesToFileAsync(Item entity, ICollection<IFormFile> images)
        {
            var fileNames = await _filesService.SaveFilesAsync(images);

            if(entity.ImagePaths is null)
            {
                entity.ImagePaths = new List<string>();
            }

            ((List<string>)entity.ImagePaths).AddRange(fileNames);
        }

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

        public async Task AddImagesAsync(string id, ICollection<IFormFile> images)
        {
            var itemToBeUpdated = await _unitOfWork.Set<Item>().Where(item => item.Id == id).Select(item => new Item() { ImagePaths = item.ImagePaths }).FirstOrDefaultAsync();

            if (itemToBeUpdated is null)
            {
                throw new NotFoundException();
            }

            itemToBeUpdated.Id = id;

            await AddImagesToFileAsync(itemToBeUpdated, images);

            _unitOfWork.Set<Item>().Update(itemToBeUpdated);
            await _unitOfWork.SaveChangesAsync();
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

        public async Task RemoveImagesAsync(string id, ICollection<string> imagePaths)
        {
            var itemToBeUpdated = await _unitOfWork.Set<Item>().Where(item => item.Id == id).Select(item => new Item() { ImagePaths = item.ImagePaths }).FirstOrDefaultAsync();

            if (itemToBeUpdated is null) 
            {
                throw new NotFoundException();
            }

            itemToBeUpdated.Id = id;

            RemoveImagesFromItem(itemToBeUpdated, imagePaths);

            _unitOfWork.Set<Item>().Update(itemToBeUpdated);
            await _unitOfWork.SaveChangesAsync();
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
                RemoveImagesFromItem(updatedEntity, dto.ImagePathsToDelete);
            }

            if (dto.ImagesToCreate is not null)
            {
                await AddImagesToFileAsync(updatedEntity, dto.ImagesToCreate);
            }

            _unitOfWork.Set<Item>().Update(updatedEntity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"{_userResolverService.GetUserName} updated the Item with id: {id}");
        }
    }
}