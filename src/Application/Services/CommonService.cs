using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Common;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sieve.Models;
using Sieve.Services;

namespace Application.Services
{
    internal abstract class CommonService<T, TRequest, TResponse> : ICommonService<TRequest, TResponse> where T : BaseEntity
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IMapper _mapper;
        protected readonly ISieveProcessor _sieveProcessor;
        private readonly ILogger _logger;

        public CommonService(IUnitOfWork unitOfWork, IMapper mapper, ISieveProcessor sieveProcessor, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _sieveProcessor = sieveProcessor;
            _logger = logger;
        }

        public virtual async Task<IEnumerable<TResponse>> GetAllAsync()
        {
            var entities = _unitOfWork.Set<T>()
                .AsNoTracking()
                .ProjectTo<TResponse>(_mapper.ConfigurationProvider);

            return await entities.ToArrayAsync();
        }
        public virtual async Task<PagedResponse<TResponse>> GetPageAsync(SieveModel sieveModel)
        {
            var entities = _unitOfWork.Set<T>()
                .AsNoTracking();

            var pageResponse = await PagedResponse<TResponse>.GetPagedResponse(entities, sieveModel, _sieveProcessor, _mapper);

            return pageResponse;
        }
        public virtual async Task<TResponse> GetByIdAsync(string id)
        {
            var existedEntity = await _unitOfWork.Set<T>().FindAsync(id);

            if (existedEntity is null)
            {
                throw new NotFoundException();
            }

            return _mapper.Map<TResponse>(existedEntity);
        }
        public virtual async Task<TResponse> AddAsync(TRequest dto)
        {
            var entity = _mapper.Map<T>(dto);
            var entityEntry = await _unitOfWork.Set<T>().AddAsync(entity);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TResponse>(entityEntry.Entity);
        }
        public virtual async Task UpdateAsync(string id, TRequest dto)
        {
            var existedEntity = await _unitOfWork.Set<T>().FindAsync(id);

            if (existedEntity is null)
            {
                throw new NotFoundException();
            }

            var updatedEntity = _mapper.Map(dto, existedEntity);

            _unitOfWork.Set<T>().Update(updatedEntity);

            await _unitOfWork.SaveChangesAsync();
        }
        public virtual async Task RemoveAsync(string id)
        {
            var existedEntity = await _unitOfWork.Set<T>().FindAsync(id);

            if (existedEntity is null)
            {
                throw new NotFoundException();
            }

            _unitOfWork.Set<T>().Remove(existedEntity);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task AddRangeAsync(IEnumerable<TRequest> dtos)
        {
            var entities = _mapper.Map<IEnumerable<T>>(dtos);

            await _unitOfWork.Set<T>().AddRangeAsync(entities);

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task RemoveRangeAsync(IEnumerable<string> keys)
        {
            List<T> entities = new List<T>();

            T? model;

            foreach (var key in keys)
            {
                model = _unitOfWork.Set<T>().Find(key);

                if (model is null)
                {
                    throw new NotFoundException();
                }

                entities.Add(model);
            }

            _unitOfWork.Set<T>().RemoveRange(entities);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}