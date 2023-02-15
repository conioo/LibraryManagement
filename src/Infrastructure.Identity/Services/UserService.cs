using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;
using Application.Exceptions;
using Application.Interfaces;
using AutoMapper;
using Infrastructure.Identity.Data;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Sieve.Services;
using System.Security.Claims;

namespace Application.Services
{
    internal class UserService : IdentityCommonService<ApplicationUser, RegisterRequest, UserResponse>, IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(IdentityContext identityContext, IMapper mapper, ISieveProcessor sieveProcessor, UserManager<ApplicationUser> userManager) : base(identityContext, mapper, sieveProcessor)
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
        }

        public override async Task<UserResponse> AddAsync(RegisterRequest dto)
        {
            var user = _mapper.Map<ApplicationUser>(dto);

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors);
            }

            return _mapper.Map<UserResponse>(user);
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
        }
    }
}