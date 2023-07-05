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
        public async Task SaveFilesAsync_ForValidFormFiles_SaveFileCorrectly()
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
        public async Task SaveFilesAsync_ForValidFormFilesButNoDirectory_SaveFileCorrectly()
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
        public async Task SaveFilesAsync_ForValidFormFilesButOneFileBeNull_SaveFileCorrectly()
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

        public void Dispose()
        {
            Directory.Delete(_testFolderPath, true);
        }
    }
}