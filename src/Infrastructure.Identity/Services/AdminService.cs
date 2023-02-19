using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;
using Application.Exceptions;
using Application.Interfaces;
using AutoMapper;
using Domain.Settings;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Extensions;
using Infrastructure.Identity.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Infrastructure.Identity.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ApplicationSettings _applicationOptions;

        public AdminService(UserManager<ApplicationUser> userManager, IMapper mapper, IEmailService emailService, IOptions<ApplicationSettings> options)
        {
            _userManager = userManager;
            _mapper = mapper;
            _emailService = emailService;
            _applicationOptions = options.Value;
        }
        public async Task<UserResponse> AddAdmin(RegisterRequest dto)
        {
            var userResponse = await CreateUser(dto, new string[] {UserRoles.Admin});

            return userResponse;
        }
        
        public async Task<UserResponse> AddWorker(RegisterRequest dto)
        {
            var userResponse = await CreateUser(dto, new string[] { UserRoles.Worker });

            return userResponse;
        }

        private async Task<UserResponse> CreateUser(RegisterRequest registerRequest, string[] roles)
        {
            var newUser = _mapper.Map<ApplicationUser>(registerRequest);

            var result = await _userManager.CreateAsync(newUser, registerRequest.Password);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors);
            }

            result = await _userManager.AddToRolesAsync(newUser, roles);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors);
            }

            await _emailService.SendVerificationEmail(newUser, _userManager, _applicationOptions);

            var userResponse = _mapper.Map<UserResponse>(newUser);

            return userResponse;
        }
    }
}
