using Application.Interfaces;
using Domain.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services
{
    public class FilesService : IFilesService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FilesService> _logger;
        private readonly PathSettings _pathSettings;
        private readonly string _itemImagesDirectoryPath;

        public FilesService(IWebHostEnvironment environment, IOptions<PathSettings> options, ILogger<FilesService> logger)
        {
            _environment = environment;
            _logger = logger;
            _pathSettings = options.Value;

            _itemImagesDirectoryPath = Path.Combine(_environment.WebRootPath, _pathSettings.PathForImagesOfItems);
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

            if (Directory.Exists(_itemImagesDirectoryPath) is false)
            {
                Directory.CreateDirectory(_itemImagesDirectoryPath);
            }

            var filePath = Path.Combine(_itemImagesDirectoryPath, uniqueFileName);

            using(var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            _logger.LogInformation("Saved {0} file", uniqueFileName);

            return uniqueFileName;
        }
        public async Task<ICollection<string>> SaveFilesAsync(ICollection<IFormFile> files)
        {
            if (Directory.Exists(_itemImagesDirectoryPath) is false)
            {
                Directory.CreateDirectory(_itemImagesDirectoryPath);
            }

            var fileNames = new List<string>();

            foreach (var file in files)
            {
                if(file is null)
                {
                    continue;
                }

                var uniqueFileName = GetUniqueFileName(file.FileName);

                var filePath = Path.Combine(_itemImagesDirectoryPath, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                fileNames.Add(uniqueFileName);
            }

            _logger.LogInformation("Saved {0} files", String.Join(' ', fileNames));

            return fileNames;
        }
        public void RemoveFile(string fileName)
        {
            if (Directory.Exists(_itemImagesDirectoryPath) is false)
            {
                return;
            }

            var filePath = Path.Combine(_itemImagesDirectoryPath, fileName);

            File.Delete(filePath);

            _logger.LogInformation("Deleted {0} file", fileName);
        }
        public void RemoveFiles(ICollection<string> fileNames)
        {
            if (Directory.Exists(_itemImagesDirectoryPath) is false)
            {
                return;
            }

            foreach (var fileName in fileNames)
            {
                var filePath = Path.Combine(_itemImagesDirectoryPath, fileName);

                File.Delete(filePath);
            }

            _logger.LogInformation("Deleted {0} files", String.Join(' ', fileNames));
        }
    }
}