using Application.Interfaces;
using Domain.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Application.Services
{
    public class FilesService : IFilesService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FilesService> _logger;
        private readonly PathSettings _pathSettings;

        public FilesService(IWebHostEnvironment environment, IOptions<PathSettings> options, ILogger<FilesService> logger)
        {
            _environment = environment;
            _logger = logger;
            _pathSettings = options.Value;
        }
        private string GetUniqueFileName(string fileName)
        {
            return string.Concat(Path.GetFileNameWithoutExtension(fileName)
                                , "_"
                                , Guid.NewGuid().ToString().AsSpan(0, 5)
                                , Path.GetExtension(fileName));
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            var uniqueFileName = GetUniqueFileName(file.FileName);

            var filePath = Path.Combine(_environment.WebRootPath, _pathSettings.PathForImagesOfItems, uniqueFileName);

            
            //Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            await file.CopyToAsync(new FileStream(filePath, FileMode.Create));

            // do bazy danych



            return uniqueFileName;
        }

        public Task<ICollection<string>> SaveFilesAsync(ICollection<IFormFile> files)
        {
            throw new NotImplementedException();
        }
    }
}
