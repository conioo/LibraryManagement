using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sieve.Models;
using Sieve.Services;

namespace Infrastructure.Identity.Services
{
    internal abstract class IdentityCommonService<T, TRequest, TResponse> : CommonService<T, TRequest, TResponse> where T : class
    {
        internal readonly IdentityContext _identityContext;

        protected IdentityCommonService(IdentityContext identityContext, IMapper mapper, ISieveProcessor sieveProcessor, IUserResolverService userResolverService, ILogger logger) : base(identityContext, mapper, sieveProcessor, logger, userResolverService)
        {
            _identityContext = identityContext;
        }
    }
}
