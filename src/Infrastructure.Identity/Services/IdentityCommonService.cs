using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace Infrastructure.Identity.Services
{
    internal abstract class IdentityCommonService<T, TRequest, TResponse> : ICommonService<TRequest, TResponse> where T : class
    {
        protected readonly IdentityContext _identityContext;
        protected readonly IMapper _mapper;
        protected readonly ISieveProcessor _sieveProcessor;

        protected IdentityCommonService(IdentityContext identityContext, IMapper mapper, ISieveProcessor sieveProcessor)
        {
            _identityContext = identityContext;
            _mapper = mapper;
            _sieveProcessor = sieveProcessor;
        }

        public virtual async Task<IEnumerable<TResponse>> GetAllAsync()
        {
            var entities = _identityContext.Set<T>()
                .AsNoTracking()
                .ProjectTo<TResponse>(_mapper.ConfigurationProvider);

            return await entities.ToArrayAsync();
        }
        public virtual async Task<PagedResponse<TResponse>> GetPageAsync(SieveModel sieveModel)
        {
            var entities = _identityContext.Set<T>()
                .AsNoTracking();

            var pageResponse = await PagedResponse<TResponse>.GetPagedResponse(entities, sieveModel, _sieveProcessor, _mapper);

            return pageResponse;
        }
        public virtual async Task<TResponse> GetByIdAsync(string id)
        {
            var existedEntity = await _identityContext.Set<T>().FindAsync(id);

            if (existedEntity is null)
            {
                throw new NotFoundException();
            }

            return _mapper.Map<TResponse>(existedEntity);
        }
        public virtual async Task<TResponse> AddAsync(TRequest dto)
        {
            var entity = _mapper.Map<T>(dto);
            var entityEntry = await _identityContext.Set<T>().AddAsync(entity);

            await _identityContext.SaveChangesAsync();

            return _mapper.Map<TResponse>(entityEntry.Entity);
        }
        public virtual async Task UpdateAsync(string id, TRequest dto)
        {
            var existedEntity = await _identityContext.Set<T>().FindAsync(id);

            if (existedEntity is null)
            {
                throw new NotFoundException();
            }

            var updatedEntity = _mapper.Map(dto, existedEntity);

            _identityContext.Set<T>().Update(updatedEntity);

            await _identityContext.SaveChangesAsync();
        }
        public virtual async Task RemoveAsync(string id)
        {
            var existedEntity = await _identityContext.Set<T>().FindAsync(id);

            if (existedEntity is null)
            {
                throw new NotFoundException();
            }

            _identityContext.Set<T>().Remove(existedEntity);
            await _identityContext.SaveChangesAsync();
        }

        public Task AddRangeAsync(IEnumerable<TRequest> dtos)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRangeAsync(IEnumerable<string> keys)
        {
            throw new NotImplementedException();
        }
    }
}
