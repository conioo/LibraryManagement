#pragma warning disable CS8618
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace Application.Dtos.Response
{
    public class PagedResponse<TResponse>
    {
        public IEnumerable<TResponse> Items { get; set; }
        public int TotalItemsCount { get; set; }
        public int TotalPages { get; set; }

        public PagedResponse()
        {
        }

        public static async Task<PagedResponse<TResponse>> GetPagedResponse<T>(IQueryable<T> entities, SieveModel sieveModel, ISieveProcessor sieveProcessor, IMapper mapper)
        {
            // filtering, sorting
            var targetEntities = sieveProcessor
               .Apply(sieveModel, entities, applyPagination: false);

            var totalCount = await targetEntities
                .CountAsync();

            var entityPage = await sieveProcessor
                .Apply(sieveModel, targetEntities, applyFiltering: false, applyPagination: true, applySorting: false)
                .ProjectTo<TResponse>(mapper.ConfigurationProvider)
                .ToListAsync();

            var pagedResponse = new PagedResponse<TResponse>();

            pagedResponse.Items = entityPage;
            pagedResponse.TotalItemsCount = totalCount;
            pagedResponse.TotalPages = (int)Math.Ceiling(totalCount / (double)sieveModel.PageSize.Value);

            return pagedResponse;
        }
    }
}
