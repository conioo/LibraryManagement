using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;
using System.Security.Claims;

namespace Application.Interfaces
{
    public interface IAccountService
    {
        public Task<UserResponse> RegisterAsync(RegisterRequest dto);
        public Task ConfirmEmailAsync(string userId, string code);
        public Task<LoginResponse> LoginAsync(LoginRequest dto);
        public Task ForgotPasswordAsync(ForgotPasswordRequest dto);
        public Task ResetPasswordAsync(ResetPasswordRequest dto);
        public Task ChangePasswordAsync(ChangePasswordRequest dto, ClaimsPrincipal claimsPrincipal);
        public Task<string> RefreshToken(string refreshToken);
        public Task Logout(ClaimsPrincipal principal);
    }
}
