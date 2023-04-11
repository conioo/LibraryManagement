using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;
using Application.Dtos.Request;
using System.Security.Claims;

namespace Application.Interfaces
{
    public interface IUserService : ICommonService<RegisterRequest, UserResponse>
    {
        public Task<UserResponse> GetUserAsync(ClaimsPrincipal principal);
        public Task UpdateAsync(ClaimsPrincipal principal, UpdateUserRequest dto);

        public Task BindProfil(string userId, string profilId, ProfileRequest dto);
        public Task<string> GetEmail(string userId);
    }
}
