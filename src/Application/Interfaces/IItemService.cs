﻿using Application.Dtos.Request;
using Application.Dtos.Response;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IItemService : ICommonService<ItemRequest, ItemResponse>
    {
        public Task<IEnumerable<CopyResponse>> GetAllCopiesAsync(string id);
        public Task<IEnumerable<CopyResponse>> GetAvailableCopiesAsync(string id);
        public Task AddImagesAsync(string id, ICollection<IFormFile> images);
        public Task RemoveImagesAsync(string id, ICollection<string> imagePaths);
        public Task UpdateAsync(string id, UpdateItemRequest dto);
    }
}