using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Services
{
    public class UserResolverService : IUserResolverService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserResolverService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string? GetUserName
        {
            get
            {
                if (_httpContextAccessor.HttpContext is null)
                {
                    return null;
                }

                if (_httpContextAccessor.HttpContext.User.Identity is null)
                {
                    return null;
                }

                return _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
            }
        }
    }
}