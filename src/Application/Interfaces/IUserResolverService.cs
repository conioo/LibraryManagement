namespace Application.Interfaces
{
    public interface IUserResolverService
    {
        public string? GetUserName { get; }
        public string? GetUserId { get; }
        public string? GetProfileCardNumber { get; }
    }
}
