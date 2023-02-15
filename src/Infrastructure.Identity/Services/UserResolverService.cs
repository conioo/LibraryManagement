using Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class UserResolverService : IUserResolverService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserResolverService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string? GetUserName()
        {
            if (_httpContextAccessor.HttpContext is null)
            {
                return null;
            }

            if (_httpContextAccessor.HttpContext.User.Identity is null)
            {
                return null;
            }

            return _httpContextAccessor.HttpContext.User.Identity.Name;
        }
    }
}