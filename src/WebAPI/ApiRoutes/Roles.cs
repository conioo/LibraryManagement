namespace WebAPI.ApiRoutes
{
    public static class Roles
    {
        public const string Prefix = "roles";

        public const string GetAllRoles = "all";
        public const string GetPage = "page";
        public const string GetRoleById = "";
        public const string AddRole = "add";
        public const string UpdateRole = "update";
        public const string RemoveRole = "remove";

        public const string GetUsersInRole = "users-in-role";
        public const string GetRolesByUser = "roles-by-user";
        public const string AddUsersToRole = "add-users-to-role";
        public const string RemoveRoleFromUsers = "remove-role-from-users";
    }
}
