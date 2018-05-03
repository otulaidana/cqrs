namespace CqrsAccount.Domain.Tests.Common
{
    using Xunit;

    [CollectionDefinition("AggregateTest")]
    public sealed class AggregateTestCollection : ICollectionFixture<EventStoreFixture>
    {
    }
}
