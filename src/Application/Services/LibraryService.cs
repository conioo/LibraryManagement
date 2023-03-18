using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Sieve.Services;

namespace Application.Services
{
    internal class LibraryService : CommonService<Library, LibraryRequest, LibraryResponse>, ILibraryService
    {
        public LibraryService(IUnitOfWork unitOfWork, IMapper mapper, ISieveProcessor sieveProcessor, ILogger<LibraryService> logger, IUserResolverService userResolverService) : base(unitOfWork, mapper, sieveProcessor, logger, userResolverService) { }
    }
}
