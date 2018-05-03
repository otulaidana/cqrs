namespace CqrsAccount.Domain.Tests.Common
{
    using System;
    using EventStore.ClientAPI;
    using EventStore.ClientAPI.Embedded;
    using EventStore.Core;
    using ReactiveDomain;
    using ReactiveDomain.EventStore;
    using ReactiveDomain.Foundation;
    using ReactiveDomain.Foundation.EventStore;

    public class EventStoreFixture : IDisposable
    {
        readonly StreamStoreRepository _repo;
        readonly ClusterVNode _node;

        public EventStoreFixture()
        {
            _node = EmbeddedVNodeBuilder
                .AsSingleNode()
                .OnDefaultEndpoints()
                .RunInMemory()
                .DisableDnsDiscovery()
                .DisableHTTPCaching()
                //.DisableScavengeMerging()
                .DoNotVerifyDbHashes()
                .Build();

            _node.StartAndWaitUntilReady().Wait();

            var conns = ConnectionSettings.Create()
                .SetDefaultUserCredentials(new EventStore.ClientAPI.SystemData.UserCredentials("admin", "changeit"))
                .Build();

            var eventStoreConnection = EmbeddedEventStoreConnection.Create(_node, conns);

            StreamStoreConnection = new EventStoreConnectionWrapper(eventStoreConnection);

            EventSerializer = new JsonMessageSerializer();
            StreamNameBuilder = new PrefixedCamelCaseStreamNameBuilder("masterdata");

            _repo = new StreamStoreRepository(StreamNameBuilder, StreamStoreConnection, EventSerializer);
        }

        public void Dispose()
        {
            StreamStoreConnection.Close();
            StreamStoreConnection.Dispose();

            _node.Stop();
        }

        public IRepository Repository => _repo;

        public IStreamStoreConnection StreamStoreConnection { get; }

        public IStreamNameBuilder StreamNameBuilder { get; }

        public IEventSerializer EventSerializer { get; }
    }
}
