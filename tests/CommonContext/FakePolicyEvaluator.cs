using Infrastructure.Identity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CommonContext
{
    public class FakePolicyEvaluator : IPolicyEvaluator
    {
        private readonly IdentityContext _identityContext;

        public FakePolicyEvaluator(IdentityContext identityContext)
        {
            _identityContext = identityContext;
        }

        public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
        {
            var user = _identityContext.Users.Single(user => user.UserName == "default");

            var claimsPrincipal = new ClaimsPrincipal();

            claimsPrincipal.AddIdentity(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
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