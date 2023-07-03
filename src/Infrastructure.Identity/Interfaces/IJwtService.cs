using Infrastructure.Identity.Entities;
using System.IdentityModel.Tokens.Jwt;

namespace Infrastructure.Identity.Interfaces
{
    public interface IJwtService
    {
        public Task<JwtSecurityToken> GenerateJwtAsync(ApplicationUser user);
        public RefreshToken GenerateRefreshToken(ApplicationUser user);
        public Task<bool> IsActiveCurrent();
        public Task DeactivateCurrent();
        public Task<bool> IsActive(string token);
        public Task Deactivate(string token);
    }
}