﻿using Application.Dtos;
using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;
using Application.Exceptions;
using Application.Interfaces;
using AutoMapper;
using Domain.Settings;
using Infrastructure.Identity.Data;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Extensions;
using Infrastructure.Identity.Interfaces;
using Infrastructure.Identity.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Identity.Services
{
    internal class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<AccountService> _logger;
        private readonly IJwtService _jwtService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ApplicationSettings _applicationOptions;
        private readonly IdentityContext _identityContext;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountService(IdentityContext identityContext, UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            IOptions<ApplicationSettings> applicationOptions,
            IMapper mapper,
            ILogger<AccountService> logger)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _emailService = emailService;
            _applicationOptions = applicationOptions.Value;
            _identityContext = identityContext;
            _signInManager = signInManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest dto, ClaimsPrincipal claimsPrincipal)
        {
            var user = await _userManager.GetUserAsync(claimsPrincipal);

            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors);
            }

            _logger.LogInformation($"Change password for {claimsPrincipal.Claims.First(claim => claim.Type == ClaimTypes.Name).Value}");
        }
        public async Task ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                throw new NotFoundException();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors);
            }

            _logger.LogInformation($"Confirm email for {user.UserName}");
        }
        public async Task<UserResponse> RegisterAsync(RegisterRequest dto)
        {
            var newUser = _mapper.Map<ApplicationUser>(dto);

            var result = await _userManager.CreateAsync(newUser, dto.Password);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors);
            }

            result = await _userManager.AddToRoleAsync(newUser, UserRoles.Basic);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors);
            }

            await _emailService.SendVerificationEmail(newUser, _userManager, _applicationOptions);

            var userResponse = _mapper.Map<UserResponse>(newUser);

            _logger.LogInformation($"Register user: {dto.UserName}");

            return userResponse;
        }
        public async Task<LoginResponse> LoginAsync(LoginRequest dto)
        {
            var userInfo = await _identityContext.Users.Where(user => user.Email == dto.Email).Select(user => new { user, user.RefreshTokenId }).FirstOrDefaultAsync();

            if (userInfo is null)
            {
                throw new AuthenticationException("Invalid email or password");
            }

            var result = await _signInManager.PasswordSignInAsync(userInfo.user, dto.Password, false, false);

            if (!result.Succeeded)
            {
                throw new AuthenticationException("Invalid email or password");
            }

            var token = await _jwtService.GenerateJwtAsync(userInfo.user);
            var tokenHandler = new JwtSecurityTokenHandler();

            var refreshToken = _jwtService.GenerateRefreshToken(userInfo.user);

            if (userInfo.RefreshTokenId is not null)
            {
                _identityContext.RefreshTokens.Remove(new RefreshToken() { Id = userInfo.RefreshTokenId });
            }

            await _identityContext.RefreshTokens.AddAsync(refreshToken);
            await _identityContext.SaveChangesAsync();

            var response = new LoginResponse
            {
                UserId = userInfo.user.Id,
                Jwt = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken.Token,
            };

            return response;
        }
        public async Task ForgotPasswordAsync(ForgotPasswordRequest dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
            {
                throw new NotFoundException();
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = new Uri(new Uri(_applicationOptions.BaseAddress), _applicationOptions.CallbackUrlForForgottenPassword).ToString();

            callbackUrl = QueryHelpers.AddQueryString(callbackUrl, "token", resetToken);

            await _emailService.SendAsync(new Email(user.Email, "Reset Password", $"Please reset your password by visiting this URL {callbackUrl}"));

            _logger.LogInformation($"Forgot password for: {user.UserName}");
        }
        public async Task ResetPasswordAsync(ResetPasswordRequest dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new NotFoundException();
            }

            var result = await _userManager.ResetPasswordAsync(user, dto.PasswordResetToken, dto.NewPassword);

            if (!result.Succeeded)
            {
                throw new IdentityException(result.Errors);
            }

            _logger.LogInformation($"Reset password for: {user.UserName}");
        }
        public async Task<string> RefreshToken(string refreshToken)
        {
            var storedTokenInfo = await _identityContext.Set<RefreshToken>().Where(entity => entity.Token == refreshToken).Select(refreshToken => new {refreshToken.IsActive, refreshToken.ApplicationUser}).FirstOrDefaultAsync();

            if (storedTokenInfo is null || storedTokenInfo.IsActive is false)
            {
                throw new AuthenticationException("refresh token is not correct");
            }

            var newJwT = await _jwtService.GenerateJwtAsync(storedTokenInfo.ApplicationUser);

            var tokenHandler = new JwtSecurityTokenHandler();

            return tokenHandler.WriteToken(newJwT);
        }
        public async Task Logout(ClaimsPrincipal principal)
        {
            var userId = _userManager.GetUserId(principal);

            var userInfo = await _identityContext.Users.Where(user => user.Id == userId).Select(user => new { user.RefreshTokenId }).FirstOrDefaultAsync();

            if (userInfo?.RefreshTokenId is not null)
            {
                var modifiedRefreshToken = new RefreshToken() { Id =  userInfo.RefreshTokenId, Revoke = true };
                _identityContext.Entry(modifiedRefreshToken).Property(refreshToken => refreshToken.Revoke).IsModified = true;
                await _identityContext.SaveChangesAsync();
            }

            await _jwtService.DeactivateCurrent();
        }
    }
}