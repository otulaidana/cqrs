namespace CqrsAccount.Domain.Tests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ReactiveDomain;
    using ReactiveDomain.Foundation;
    using ReactiveDomain.Messaging;
    using ReactiveDomain.Messaging.Bus;
    using Xunit.ScenarioReporting;

    /// <summary>
    /// TODO: work out why this isn't working properly
    /// </summary>
    /// <typeparam name="TAggregate"></typeparam>
    public class EventStoreScenarioRunner<TAggregate> : ReflectionBasedScenarioRunner<Event, Command, Event>, IDisposable
        where TAggregate : EventDrivenStateMachine
    {
        readonly EventStoreFixture _fixture;
        readonly Guid _id;
        readonly IDisposable _disposable;
        readonly IDispatcher _bus;
        readonly string _streamName;
        long _readFromEvent;

        public EventStoreScenarioRunner(Guid aggregateId, EventStoreFixture fixture, Func<IRepository, IDispatcher, IDisposable> init)
        {
            _id = aggregateId;
            _fixture = fixture;
            _streamName = _fixture.StreamNameBuilder.GenerateForAggregate(typeof(TAggregate), _id);

            _bus = new Dispatcher("Test bus", watchSlowMsg: true, slowMsgThreshold: TimeSpan.FromSeconds(1), slowCmdThreshold: TimeSpan.FromSeconds(1));

            _disposable = init(_fixture.Repository, _bus);

            this.Configure(
                configure =>
                {
                    configure.ForType<Message>(
                        ct =>
                        {
                            ct.Ignore(m => m.MsgId);
                        });
                });
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        protected override Task Given(IReadOnlyList<Event> givens)
        {
            if (givens.Count == 0)
            {
                _readFromEvent = 0;
                return Task.CompletedTask;
            }

            if (_fixture.Repository.TryGetById<TAggregate>(_id, out var _))
                throw new InvalidOperationException("Aggregate stream already exists prior to test run");

            var newEvents = givens;

            var writeResult = _fixture.StreamStoreConnection.AppendToStream(
                _streamName,
                EventStore.Core.Data.ExpectedVersion.NoStream,
                null,
                newEvents.Select(e => _fixture.EventSerializer.Serialize(e)).ToArray()
            );

            _readFromEvent = writeResult.NextExpectedVersion + 1;

            return Task.CompletedTask;
        }

        protected override Task When(Command when)
        {
            // If running in debugger...
            try
            {
                _bus.Fire(when);
            }
            catch (CommandException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
            return Task.CompletedTask;
        }

        protected override Task<IReadOnlyList<Event>> ActualResults()
        {
            var slice = _fixture.StreamStoreConnection.ReadStreamForward(
                _streamName,
                _readFromEvent,
                // Greater than 4096, we need to page, but this should not be a problem in tests
                4096
            );

            IReadOnlyList<Event> events = slice.Events.Select(e => _fixture.EventSerializer.Deserialize(e)).OfType<Event>().ToArray();
            return Task.FromResult(events);
        }
    }
}
