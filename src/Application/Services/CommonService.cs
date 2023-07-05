using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Helpers;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sieve.Models;
using Sieve.Services;

namespace Application.Services
{
    public abstract class CommonService<T, TRequest, TResponse> : ICommonService<TRequest, TResponse> where T : class
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IMapper _mapper;
        protected readonly ISieveProcessor _sieveProcessor;
        protected readonly ILogger _logger;
        protected readonly IUserResolverService _userResolverService;

        public CommonService(IUnitOfWork unitOfWork, IMapper mapper, ISieveProcessor sieveProcessor, ILogger logger, IUserResolverService userResolverService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _sieveProcessor = sieveProcessor;
            _logger = logger;
            _userResolverService = userResolverService;
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

            var pageResponse = await PagedResponse<TResponse>.GetPagedResponseAsync(entities, sieveModel, _sieveProcessor, _mapper);

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
        public virtual Task<TResponse> AddAsync(TRequest dto)
        {
            return AddAsync(_mapper.Map<T>(dto));
        }
        public virtual async Task<TResponse> AddAsync(T entity)
        {
            await _unitOfWork.Set<T>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"{_userResolverService.GetUserName} added the {typeof(T).Name} with id: {EntityHelper.GetKeyValue(entity)}");

            return _mapper.Map<TResponse>(entity);
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

            _logger.LogInformation($"{_userResolverService.GetUserName} updated the {typeof(T).Name} with id: {id}");
        }
        public virtual async Task RemoveAsync(string id)
        {
            var entity = EntityHelper.GetEntityWithKey<T>(id);

            _unitOfWork.Set<T>().Remove(entity);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new NotFoundException();
            }

            _logger.LogInformation($"{_userResolverService.GetUserName} removed the {typeof(T).Name} with id: {id}");
        }
        public virtual  Task AddRangeAsync(ICollection<TRequest> dtos)
        {
            var entities = _mapper.Map<ICollection<T>>(dtos);

            return AddRangeAsync(entities);

        }
        public virtual async Task AddRangeAsync(ICollection<T> entities)
        {
            await _unitOfWork.Set<T>().AddRangeAsync(entities);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"{_userResolverService.GetUserName} added {entities.Count} the {typeof(T).Name}");
        }
        public virtual async Task RemoveRangeAsync(IEnumerable<string> keys)
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

            //foreach (var key in keys)
            //{
            //    var entity = EntityHelper.GetEntityWithKey<T>(key);

            //    _unitOfWork.Entry(entity).State = EntityState.Deleted;
            //}

            //try
            //{
            //    await _unitOfWork.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    throw new NotFoundException();
            //}

            _logger.LogInformation($"{_userResolverService.GetUserName} removed {keys.Count()} the {typeof(T).Name}");
        }
    }
}