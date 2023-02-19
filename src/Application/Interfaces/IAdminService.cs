using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;

namespace Application.Interfaces
{
    public interface IAdminService
    {
        public Task<UserResponse> AddAdmin(RegisterRequest dto);
        public Task<UserResponse> AddWorker(RegisterRequest dto);
    }
}
