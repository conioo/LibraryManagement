using WebAPITests.Integration;

namespace CommonContext.SharedContextBuilders
{
    public interface ISharedContextBuilder
    {
        public SharedContext Value { get; }
    }
}
