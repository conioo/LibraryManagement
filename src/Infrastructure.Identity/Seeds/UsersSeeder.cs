using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity.Seeds
{
    public static class UsersSeeder
    {
        public static void SeedDefaultUsers(this ModelBuilder builder)
        {
            var userAdmin = new ApplicationUser
            {
                UserName = "Admin",
                NormalizedUserName = "ADMIN",
                FirstName = "Admin",
                LastName = "Admin",
                Email = "defaultadmin@gmail.com",
                NormalizedEmail = "DEFAULTADMIN@GMAIL.COM",
                EmailConfirmed = true,
            };

            var passwordHasher = new PasswordHasher<ApplicationUser>();

            userAdmin.PasswordHash = passwordHasher.HashPassword(userAdmin, "11js&5$henai83s");

            builder.Entity<ApplicationUser>().HasData(userAdmin);

            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>() { UserId = userAdmin.Id, RoleId = "051a944a-9862-4fc6-a2e5-714c13585122" });
        }
    }
}
