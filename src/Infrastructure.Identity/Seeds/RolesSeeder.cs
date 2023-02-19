using Infrastructure.Identity.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

            roleAdmin.Id = "E7FE5A9D-2647-48D5-8CAF-EE6597ED7283";

            builder.Entity<IdentityRole>().HasData(roleAdmin, roleModerator, roleWorker, roleBasic);
        }
    }
}