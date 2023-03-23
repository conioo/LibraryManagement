using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Sieve.Services;
using static Sieve.Extensions.MethodInfoExtended;

namespace Application.Services
{
    internal class CopyService : CommonService<Copy, CopyRequest, CopyResponse>, ICopyService
    {
        public CopyService(IUnitOfWork unitOfWork, IMapper mapper, ISieveProcessor sieveProcessor, ILogger<CopyService> logger, IUserResolverService userResolverService) : base(unitOfWork, mapper, sieveProcessor, logger, userResolverService)
        { }

        public override async Task<CopyResponse> AddAsync(CopyRequest dto)
        {
            var item = await _unitOfWork.Set<Item>().FindAsync(dto.ItemId);

            if (item is null)
            {
                throw new NotFoundException();
            }

            var library = await _unitOfWork.Set<Library>().FindAsync(dto.LibraryId);

            if (library is null)
            {
                throw new NotFoundException();
            }

            var copies = new List<Copy>();

            for (int i = 0; i < dto.Count; ++i)
            {
                copies.Add(new Copy { Item = item, Library = library });
            }

            await _unitOfWork.Set<Copy>().AddRangeAsync(copies);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"{_userResolverService.GetUserName} added {dto.Count} the copies '{item.Title}'");

            return null;
        }

    }
}
