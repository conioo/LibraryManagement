using WebAPITests.Integration;

namespace CommonContext.SharedContextBuilders
{
    internal interface ISharedContextBuilder
    {
        public SharedContext Value { get; }
    }
}
