using Application.Dtos;
using Application.Interfaces;
using Domain.Settings;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Infrastructure.Identity.Extensions
{
    internal static class IEmailServiceExtension
    {
        internal static async Task SendVerificationEmail(this IEmailService service, ApplicationUser user, UserManager<ApplicationUser> userManager, ApplicationSettings options)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            var callbackUrl = $"{options.BaseAddress}/{options.RoutePrefix}/{options.CallbackUrlForVerificationEmail}";

            callbackUrl = QueryHelpers.AddQueryString(callbackUrl, "userId", user.Id);
            callbackUrl = QueryHelpers.AddQueryString(callbackUrl, "token", token);

            await service.SendAsync(new Email(user.Email, "Confirm your account", $"Please confirm your account by visiting this URL {callbackUrl}"));
        }
    }
}
