using Application.Interfaces;
using Domain.Settings;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// regenerate jwt z httpcontext
namespace Application.Services
{
    internal class JwtService : IJwtService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly HttpContext? _httpContext;
        private readonly IDistributedCache _cache;

        public JwtService(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtOptions, IHttpContextAccessor httpContextAccessor, IDistributedCache cache)
        {
            _userManager = userManager;
            _jwtSettings = jwtOptions.Value;
            _cache = cache;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public async Task Deactivate(string token)
        {
            await _cache.SetStringAsync(GetKey(token), String.Empty, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_jwtSettings.ExpireInMinutes)
            });
        }

        public Task DeactivateCurrent()
        {
            return Deactivate(GetCurrentToken());
        }

        public async Task<JwtSecurityToken> GenerateJwtAsync(ApplicationUser user)
        {
            var claims = await _userManager.GetClaimsAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            claims = claims.Union(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserName", user.UserName),
            }).ToList();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.IssuerSigningKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.Now.AddMinutes(_jwtSettings.ExpireInMinutes);

            var token = new JwtSecurityToken(_jwtSettings.Issuer, _jwtSettings.Audience, claims,
                expires: expires,
                signingCredentials: credentials);

            return token;
        }
        public RefreshToken GenerateRefreshToken(ApplicationUser user)
        {
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                Created = DateTime.Now,
                Expired = DateTime.Now.AddDays(_jwtSettings.RefreshTokenExpireInDays),
                ApplicationUser = user
            };

            return refreshToken;
        }

        public async Task<bool> IsActive(string token)
        {
            return await _cache.GetStringAsync(GetKey(token)) == null;
        }

        public Task<bool> IsActiveCurrent()
        {
            return IsActive(GetCurrentToken());
        }

        private static string GetKey(string token) => $"d: {token}";
        private string GetCurrentToken()
        {
            var token = _httpContext.Request.Headers["authorization"].ToString().Split().Last();

            return token;
        }
    }
}