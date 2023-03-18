using Infrastructure.Identity.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Serilog.Core;
using static Sieve.Extensions.MethodInfoExtended;

namespace Infrastructure.Identity.Seeds
{
    public static class RolesSeeder
    {
        public static void SeedDefaultRoles(this ModelBuilder builder)
        {
            var roleAdmin = new IdentityRole(UserRoles.Admin);
            var roleModerator = new IdentityRole(UserRoles.Moderator);
            var roleWorker = new IdentityRole(UserRoles.Worker);
            var roleBasic = new IdentityRole(UserRoles.Basic);

            roleAdmin.NormalizedName = UserRoles.Admin.ToUpper();
            roleModerator.NormalizedName = UserRoles.Moderator.ToUpper();
            roleWorker.NormalizedName = UserRoles.Worker.ToUpper();
            roleBasic.NormalizedName = UserRoles.Basic.ToUpper();

            roleAdmin.Id = "051a944a-9862-4fc6-a2e5-714c13585122";
            builder.Entity<IdentityRole>().HasData(roleAdmin, roleModerator, roleWorker, roleBasic);
        }
    }
}