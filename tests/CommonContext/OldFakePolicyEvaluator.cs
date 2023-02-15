using Infrastructure.Identity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CommonContext
{
    public class OldFakePolicyEvaluator : IPolicyEvaluator
    {
        private readonly IdentityContext _identityContext;

        public OldFakePolicyEvaluator(IdentityContext identityContext)
        {
            _identityContext = identityContext;
        }

        public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
        {
            var claimsPrincipal = new ClaimsPrincipal();

            claimsPrincipal.AddIdentity(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "default"),
                new Claim(ClaimTypes.Name, "default"),
                new Claim(ClaimTypes.Email, "default@example.com"),
            }));

            var ticket = new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme);

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }

        public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
        {
            var result = PolicyAuthorizationResult.Success();
            return Task.FromResult(result);
        }
    }
}