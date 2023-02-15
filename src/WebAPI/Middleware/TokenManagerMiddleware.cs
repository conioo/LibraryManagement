using Application.Exceptions;
using Application.Interfaces;
using Microsoft.Extensions.Primitives;

namespace WebAPI.Middleware
{
    public class TokenManagerMiddleware : IMiddleware
    {
        private readonly IJwtService _service;
        public TokenManagerMiddleware(IJwtService service)
        {
            _service = service;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Headers.Authorization != StringValues.Empty)
            {
                if (!(await _service.IsActiveCurrent()))
                {
                    throw new AuthenticationException("token has been revoked");
                }
            }

            await next.Invoke(context);
        }
    }
}
