using Application.Dtos.Response;
using Sieve.Models;

namespace Application.Interfaces
{
    public interface ICommonService<TRequest, TResponse>
    {
        public abstract Task<IEnumerable<TResponse>> GetAllAsync();
        public abstract Task<PagedResponse<TResponse>> GetPageAsync(SieveModel sieveModel);
        public abstract Task<TResponse> GetByIdAsync(string id);
        public abstract Task<TResponse> AddAsync(TRequest dto);
        public abstract Task AddRangeAsync(ICollection<TRequest> dtos);
        public abstract Task RemoveAsync(string id);
        public abstract Task RemoveRangeAsync(IEnumerable<string> keys);
        public abstract Task UpdateAsync(string id, TRequest dto);
    }
}