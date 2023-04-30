using Application.Dtos.Identity.Request;
using Application.Dtos.Identity.Response;
using Application.Dtos.Response;
using AutoMapper;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Mappings
{
    internal class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterRequest, ApplicationUser>();

            CreateMap<ApplicationUser, UserResponse>();
            CreateMap<UpdateUserRequest, ApplicationUser>();

            CreateMap<RoleRequest, IdentityRole>();
            CreateMap<IdentityRole, RoleResponse>();
            CreateMap<IdentityRole, RoleRequest>();
        }
    }
}
