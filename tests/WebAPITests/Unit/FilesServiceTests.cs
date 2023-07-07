using Application.Services;
using CommonContext;
using Domain.Settings;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Reflection;

namespace WebAPITests.Unit
{
    public class FilesServiceTests : IDisposable
    {
        private readonly Mock<IOptions<PathSettings>> _pathSettingsMock;
        private readonly Mock<ILogger<FilesService>> _loggerMock;
        private readonly FilesService _filesService;
        private readonly string _testFolderPath;
        private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock;

        public FilesServiceTests()
        {
            var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var contentRootPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(binPath).FullName).FullName).FullName).FullName;

            _testFolderPath = Path.Combine(contentRootPath, "testDir");

            Directory.CreateDirectory(_testFolderPath);

            _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            _webHostEnvironmentMock.SetupProperty(webHost => webHost.WebRootPath, contentRootPath);

            var pathSettings = new PathSettings() { PathForImagesOfItems = "testDir" };

            _pathSettingsMock = new Mock<IOptions<PathSettings>>();
            _pathSettingsMock.Setup(pathsettings => pathsettings.Value).Returns(pathSettings);

            _loggerMock = new Mock<ILogger<FilesService>>();

            _filesService = new FilesService(_webHostEnvironmentMock.Object, _pathSettingsMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task SaveFileAsync_ForValidFormFile_SaveFileCorrectly()
        {
            var fileForm = DataGenerator.GetImageFormFile("bookImage.png");

            var filename = await _filesService.SaveFileAsync(fileForm);

            filename.Should().NotBeNullOrEmpty();
            File.Exists(Path.Combine(_testFolderPath, filename)).Should().BeTrue();
        }

        [Fact]
        public async Task SaveFileAsync_ForValidFormFileButNoDirectory_SaveFileCorrectly()
        {
            Directory.Delete(_testFolderPath, true);

            var fileForm = DataGenerator.GetImageFormFile("bookImage.png");

            var filename = await _filesService.SaveFileAsync(fileForm);

            filename.Should().NotBeNullOrEmpty();
            File.Exists(Path.Combine(_testFolderPath, filename)).Should().BeTrue();
        }

        [Fact]
        public async Task SaveFilesAsync_ForValidFormFiles_SaveFilesCorrectly()
        {
            var fileForms = new List<IFormFile>()
            {
                DataGenerator.GetImageFormFile("1bookImage.png"),
                DataGenerator.GetImageFormFile("2bookImage.png")
            };

            var filenames = (List<string>)await _filesService.SaveFilesAsync(fileForms);

            filenames.Count.Should().Be(2);
            File.Exists(Path.Combine(_testFolderPath, filenames[0])).Should().BeTrue();
            File.Exists(Path.Combine(_testFolderPath, filenames[1])).Should().BeTrue();
        }

        [Fact]
        public async Task SaveFilesAsync_ForValidFormFilesButNoDirectory_SaveFilesCorrectly()
        {
            Directory.Delete(_testFolderPath, true);

            var fileForms = new List<IFormFile>()
            {
                DataGenerator.GetImageFormFile("1bookImage.png"),
                DataGenerator.GetImageFormFile("2bookImage.png")
            };

            var filenames = (List<string>)await _filesService.SaveFilesAsync(fileForms);

            filenames.Count.Should().Be(2);
            File.Exists(Path.Combine(_testFolderPath, filenames[0])).Should().BeTrue();
            File.Exists(Path.Combine(_testFolderPath, filenames[1])).Should().BeTrue();
        }

        [Fact]
        public async Task SaveFilesAsync_ForValidFormFilesButOneFileBeNull_SaveFilesCorrectly()
        {
            var fileForms = new List<IFormFile>()
            {
                DataGenerator.GetImageFormFile("1bookImage.png"),
                null,
                DataGenerator.GetImageFormFile("2bookImage.png")
            };

            var filenames = (List<string>)await _filesService.SaveFilesAsync(fileForms);

            filenames.Count.Should().Be(2);
            File.Exists(Path.Combine(_testFolderPath, filenames[0])).Should().BeTrue();
            File.Exists(Path.Combine(_testFolderPath, filenames[1])).Should().BeTrue();
        }

        [Fact]
        public async Task RemoveFile_ForExistingFile_RemoveFileCorrectly()
        {
            var fileForm = DataGenerator.GetImageFormFile("bookImage1.png");

            var filename = await _filesService.SaveFileAsync(fileForm);

            _filesService.RemoveFile(filename);

            File.Exists(Path.Combine(_testFolderPath, filename)).Should().BeFalse();
        }

        [Fact]
        public void RemoveFile_ForNonExistingFile_DoesNothing()
        {
            var filename = "3ja90image.png";

            _filesService.RemoveFile(filename);

            File.Exists(Path.Combine(_testFolderPath, filename)).Should().BeFalse();
        }

        [Fact]
        public void RemoveFile_ForNonExistingDirectory_DoesNothing()
        {
            Directory.Delete(_testFolderPath, true);

            var filename = "3ja90image.png";

            _filesService.RemoveFile(filename);

            Directory.Exists(_testFolderPath);

            File.Exists(Path.Combine(_testFolderPath, filename)).Should().BeFalse();
        }

        [Fact]
        public async Task RemoveFiles_ForExistingFiles_RemoveFilesCorrectly()
        {
            var fileForms = new List<IFormFile>()
            {
                DataGenerator.GetImageFormFile("bookImage1.png"),
                DataGenerator.GetImageFormFile("bookImage2.png")
            };

            var filenames = (List<string>)await _filesService.SaveFilesAsync(fileForms);

            _filesService.RemoveFiles(filenames);


            File.Exists(Path.Combine(_testFolderPath, filenames[0])).Should().BeFalse();
            File.Exists(Path.Combine(_testFolderPath, filenames[1])).Should().BeFalse();
        }

        [Fact]
        public async Task RemoveFiles_ForOneNonExistingFile_RemoveOneFileCorrectly()
        {
            var fileForms = new List<IFormFile>()
            {
                DataGenerator.GetImageFormFile("bookImage1.png"),
                DataGenerator.GetImageFormFile("bookImage2.png")
            };

            var filename = await _filesService.SaveFileAsync(fileForms[0]);

            _filesService.RemoveFiles(new List<string>() { filename, "bookImage2.png" });


            File.Exists(Path.Combine(_testFolderPath, filename)).Should().BeFalse();
            File.Exists(Path.Combine(_testFolderPath, "bookImage2.png")).Should().BeFalse();
        }

        [Fact]
        public void RemoveFiles_ForNonExistingDirectory_DoesNothing()
        {
            Directory.Delete(_testFolderPath, true);

            var filenames = new List<string>()
            {
                "3ja90image.png",
                "n49zkimage.png"
            };

            _filesService.RemoveFiles(filenames);

            File.Exists(Path.Combine(_testFolderPath, filenames[0])).Should().BeFalse();
            File.Exists(Path.Combine(_testFolderPath, filenames[1])).Should().BeFalse();
        }

        public void Dispose()
        {
            if (Directory.Exists(_testFolderPath))
            {
                Directory.Delete(_testFolderPath, true);
            }
        }
    }
}