using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;
using Application.Dtos.Request;
using Application.Exceptions;
using Application.Interfaces;
using AutoMapper;
using Infrastructure.Identity.Data;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Sieve.Services;
using System.Data;
using System.Security.Claims;

namespace Application.Services
{
    internal class UserService : IdentityCommonService<ApplicationUser, RegisterRequest, UserResponse>, IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(IdentityContext identityContext, IMapper mapper, ISieveProcessor sieveProcessor, ILogger<RoleService> logger, IUserResolverService userResolverService, UserManager<ApplicationUser> userManager) : base(identityContext, mapper, sieveProcessor, userResolverService, logger)
        {
            _userManager = userManager;
        }

        public override async Task RemoveAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new NotFoundException();
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors);
            }

            _logger.LogInformation($"{_userResolverService.GetUserName} removed user: {id}");
        }

        public async Task<UserResponse> GetUserAsync(ClaimsPrincipal principal)
        {
            var userId = _userManager.GetUserId(principal);
            var userResponse = await GetByIdAsync(userId);

            return userResponse;
        }

        public async Task UpdateAsync(ClaimsPrincipal principal, UpdateUserRequest dto)
        {
            var user = await _userManager.GetUserAsync(principal);

            if (dto.UserName is not null)
            {
                user.UserName = dto.UserName;
            }

            if (dto.FirstName is not null)
            {
                user.FirstName = dto.FirstName;
            }

            if (dto.LastName is not null)
            {
                user.LastName = dto.LastName;
            }

            _identityContext.Users.Update(user);

            await _identityContext.SaveChangesAsync();

            _logger.LogInformation($"{_userResolverService.GetUserName} updated user: {user.Id}");
        }

        public async Task BindProfil(string userId, string profileCardNumber, ProfileRequest dto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                throw new NotFoundException();
            }

            if(user.ProfileCardNumber is not null)
            {
                throw new BadRequestException($"Profile already exists for user {userId}");
            }

            user.ProfileCardNumber = profileCardNumber;
            user.PhoneNumber = dto.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded is false)
            {
                throw new IdentityException(result.Errors);
            }
        }

        public async Task<string> GetEmail(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                throw new NotFoundException();
            }

            return user.Email;
        }
    }
}