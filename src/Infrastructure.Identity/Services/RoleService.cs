using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;
using Application.Exceptions;
using Application.Interfaces;
using AutoMapper;
using Infrastructure.Identity.Data;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sieve.Services;
using System.Data;
using System.Diagnostics;

namespace Infrastructure.Identity.Services
{
    internal class RoleService : IdentityCommonService<IdentityRole, RoleRequest, RoleResponse>, IRoleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleService(IdentityContext identityContext, IMapper mapper, ISieveProcessor sieveProcessor, ILogger<RoleService> logger, IUserResolverService userResolverService, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager) : base(identityContext, mapper, sieveProcessor, userResolverService, logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task AddUsersToRoleAsync(RoleModificationRequest roleModificationRequest)
        {
            var roleName = await _identityContext.Roles.Where(role => role.Id == roleModificationRequest.RoleId).Select(role => role.Name).FirstOrDefaultAsync();

            if (roleName is null)
            {
                throw new NotFoundException($"role by id {roleModificationRequest.RoleId} doesn't exist");
            }

            var users = new List<ApplicationUser>();

            foreach (var userId in roleModificationRequest.UsersId)
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user is null)
                {
                    throw new NotFoundException($"user by id {userId} doesn't exist");
                }

                users.Add(user);
            }

            foreach (var user in users)
            {
                var result = await _userManager.AddToRoleAsync(user, roleName);

                if (!result.Succeeded)
                {
                    throw new IdentityException(result.Errors);
                }
            }

            _logger.LogInformation($"{_userResolverService.GetUserName} added role: {roleModificationRequest.RoleId} to users: {String.Join(" ", roleModificationRequest.UsersId)}");
        }
        public async Task RemoveRoleFromUsersAsync(RoleModificationRequest roleModificationRequest)
        {
            var roleName = await _identityContext.Roles.Where(role => role.Id == roleModificationRequest.RoleId).Select(role => role.Name).FirstOrDefaultAsync();

            if (roleName is null)
            {
                throw new NotFoundException($"role by id {roleModificationRequest.RoleId} doesn't exist");
            }
            var users = new List<ApplicationUser>();

            foreach (var userId in roleModificationRequest.UsersId)
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user is null)
                {
                    throw new NotFoundException($"user by id {userId} doesn't exist");
                }

                users.Add(user);
            }

            foreach (var user in users)
            {
                var result = await _userManager.RemoveFromRoleAsync(user, roleName);

                if (!result.Succeeded)
                {
                    throw new IdentityException(result.Errors);
                }
            }

            _logger.LogInformation($"{_userResolverService.GetUserName} removed role: {roleModificationRequest.RoleId} from users: {String.Join(" ", roleModificationRequest.UsersId)}");
        }
        public async Task<IEnumerable<RoleResponse>> GetRolesByUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                throw new NotFoundException();
            }

            var roles = await _userManager.GetRolesAsync(user);

            List<RoleResponse> result = new List<RoleResponse>();

            foreach (var role in roles)
            {
                RoleResponse roleResponse = new RoleResponse();
                roleResponse.Name = role;
                result.Add(roleResponse);
            }

            return result;
        }
        public async Task<IEnumerable<UserResponse>> GetUsersInRoleAsync(string roleId)
        {
            var roleName = await _identityContext.Roles.Where(role => role.Id == roleId).Select(role => role.Name).FirstOrDefaultAsync();

            if (roleName is null)
            {
                throw new NotFoundException();
            }

            var users = await _userManager.GetUsersInRoleAsync(roleName);

            var usersResponse = _mapper.Map<IEnumerable<UserResponse>>(users);

            return usersResponse;
        }
        public override async Task<RoleResponse> AddAsync(RoleRequest dto)
        {
            var role = _mapper.Map<IdentityRole>(dto);

            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors);
            }

            _logger.LogInformation($"{_userResolverService.GetUserName} added role: {role.Id}");

            return _mapper.Map<RoleResponse>(role);
        }
        public override async Task UpdateAsync(string id, RoleRequest dto)
        {
            var existedEntity = await _roleManager.FindByIdAsync(id);

            if (existedEntity is null)
            {
                throw new NotFoundException();
            }

            var updatedEntity = _mapper.Map(dto, existedEntity);

            await _roleManager.UpdateAsync(updatedEntity);

            _logger.LogInformation($"{_userResolverService.GetUserName} updated role: {id}");
        }
        public override async Task RemoveAsync(string id)
        {
            var result = await _roleManager.DeleteAsync(new IdentityRole() { Id = id});

            if(result.Succeeded is false)
            {
                throw new NotFoundException();
            }

            _logger.LogInformation($"{_userResolverService.GetUserName} removed role: {id}");
        }
    }
}