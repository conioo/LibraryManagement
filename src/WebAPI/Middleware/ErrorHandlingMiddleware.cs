using Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebAPI.Middleware
{
    public class ErrorHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
        {
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (NotFoundException exception)
            {
                context.Response.StatusCode = 404;

                if (!string.IsNullOrEmpty(exception.Message))
                {
                    await context.Response.WriteAsync(exception.Message);
                }
            }
            catch (IdentityException exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var problemDetails = new ValidationProblemDetails();

                if (exception.Errors is not null)
                {
                    foreach (var error in exception.Errors)
                    {
                        problemDetails.Errors.Add(error.Code, new string[] { error.Description });
                    }
                }

                await context.Response.WriteAsJsonAsync(problemDetails);
            }
            catch (AuthenticationException exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                await context.Response.WriteAsync(exception.Message);
            }
            catch (AuthorizationException exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                await context.Response.WriteAsync(exception.Message);
            }
            catch (BadRequestException exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                await context.Response.WriteAsync(exception.Message);
            }
            catch(NoContentException exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NoContent;

                await context.Response.WriteAsync(exception.Message);
            }
            catch (ApiException exception)
            {
                context.Response.StatusCode = 500;

                _logger.LogError(exception.Message);

                await context.Response.WriteAsync("something went wrong");
            }
            catch (Exception exception)
            {
                context.Response.StatusCode = 500;
                _logger.LogError(exception.Message);
                await context.Response.WriteAsync("something went wrong");
            }
        }
    }
}