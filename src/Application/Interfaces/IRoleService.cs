using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;

namespace Application.Interfaces
{
    public interface IRoleService : ICommonService<RoleRequest, RoleResponse>
    {
        public Task<IEnumerable<UserResponse>> GetUsersInRoleAsync(string roleId);
        public Task<IEnumerable<RoleResponse>> GetRolesByUserAsync(string userId);
        public Task AddUsersToRoleAsync(RoleModificationRequest roleModificationRequest);
        public Task RemoveRoleFromUsersAsync(RoleModificationRequest roleModificationRequest);
    }
}
