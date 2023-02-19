using Application.Dtos.Identity.Response;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.Roles;
using WebAPITests.Integration;

namespace CommonContext.SharedContextBuilders
{
    public class AuthorizationContextBuilder : ISharedContextBuilder
    {
        public readonly ApplicationUser _admin;
        public readonly ApplicationUser _moderator;
        public readonly ApplicationUser _worker;
        public readonly ApplicationUser _basic;

        public readonly LoginResponse _adminResponse;
        public readonly LoginResponse _moderatorResponse;
        public readonly LoginResponse _workerResponse;
        public readonly LoginResponse _basicResponse;

        public AuthorizationContextBuilder()
        {
            Value = new SharedContext();

            List<ApplicationUser> users = (List<ApplicationUser>)DataGenerator.Get<ApplicationUser>(3);

            _admin = Value.IdentityDbContext.Users.Single(user => user.UserName == "Admin");

            _moderator = users[0];
            _worker = users[1];
            _basic = users[2];

            _admin.EmailConfirmed = true;
            _moderator.EmailConfirmed = true;
            _worker.EmailConfirmed = true;
            _basic.EmailConfirmed = true;

            Value.UserManager.CreateAsync(_moderator, DataGenerator.GetUserPassword);
            Value.UserManager.CreateAsync(_worker, DataGenerator.GetUserPassword);
            Value.UserManager.CreateAsync(_basic, DataGenerator.GetUserPassword);

            Value.UserManager.AddToRoleAsync(_moderator, UserRoles.Moderator);
            Value.UserManager.AddToRoleAsync(_worker, UserRoles.Worker);
            Value.UserManager.AddToRoleAsync(_basic, UserRoles.Basic);

            var client = Value.CreateClient();

            _adminResponse = Value.LoginAsync(client, _admin, "11js&5$henai83s").Result;
            _moderatorResponse = Value.LoginAsync(client, _moderator, DataGenerator.GetUserPassword).Result;
            _workerResponse = Value.LoginAsync(client, _worker, DataGenerator.GetUserPassword).Result;
            _basicResponse = Value.LoginAsync(client, _basic, DataGenerator.GetUserPassword).Result;
        }
        public SharedContext Value { get; }
    }
}
