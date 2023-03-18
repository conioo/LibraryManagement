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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Identity.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ApplicationSettings _applicationOptions;
        private readonly IUserResolverService _userResolverService;
        private readonly ILogger<AdminService> _logger;

        public AdminService(UserManager<ApplicationUser> userManager, IMapper mapper, IEmailService emailService, IOptions<ApplicationSettings> options, IUserResolverService userResolverService, ILogger<AdminService> logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _emailService = emailService;
            _applicationOptions = options.Value;
            _userResolverService = userResolverService;
            _logger = logger;
        }
        public async Task<UserResponse> AddAdmin(RegisterRequest dto)
        {
            var userResponse = await CreateUser(dto, new string[] { UserRoles.Admin });


            _logger.LogInformation($"Added admin({dto.UserName}) by: {_userResolverService.GetUserName}");
            return userResponse;
        }

        public async Task<UserResponse> AddWorker(RegisterRequest dto)
        {
            var userResponse = await CreateUser(dto, new string[] { UserRoles.Worker });

            _logger.LogInformation($"Added worker({dto.UserName}) by: {_userResolverService.GetUserName}");

            return userResponse;
        }

        private async Task<UserResponse> CreateUser(RegisterRequest dto, string[] roles)
        {
            var newUser = _mapper.Map<ApplicationUser>(dto);

            var result = await _userManager.CreateAsync(newUser, dto.Password);

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

            _logger.LogInformation($"Added user({dto.UserName}) roles: {String.Join(" ", roles)}by: {_userResolverService.GetUserName}");

            return userResponse;
        }
    }
}
