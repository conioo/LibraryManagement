﻿using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IFilesService
    {
        public Task<string> SaveFileAsync(IFormFile file);
        public Task<ICollection<string>> SaveFilesAsync(ICollection<IFormFile> files);
        public void RemoveFile(string fileName);
        public void RemoveFiles(ICollection<string> fileNames);
    }
}