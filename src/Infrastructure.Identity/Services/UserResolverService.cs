using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Identity.Services
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

        public string? GetUserId
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

                return _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
            }
        }

        public string? GetProfileCardNumber
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

                var value = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == "ProfileCardNumber")?.Value;

                if(value == string.Empty)
                {
                    return null;
                }

                return value;
            }
        }
    }
}