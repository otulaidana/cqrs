namespace CqrsAccount.Domain.Tests
{
    using System;
    using System.Threading.Tasks;
    using CqrsAccount.Domain.Tests.Common;
    using CqrsAccount.Domain.Aggregates;
    using CqrsAccount.Domain.Commands;
    using CqrsAccount.Domain.Events;
    using ReactiveDomain.Messaging;
    using Xunit;
    using Xunit.ScenarioReporting;

    [Collection("AggregateTest")]
    public class SetDailyWireTransferLimitTests : IDisposable
    {
        readonly Guid _accountId;
        readonly EventStoreScenarioRunner<Account> _runner;

        public SetDailyWireTransferLimitTests(EventStoreFixture fixture)
        {
            _accountId = Guid.NewGuid();
            _runner = new EventStoreScenarioRunner<Account>(
                _accountId,
                fixture,
                (repository, dispatcher) => new AccountCommandHandler(repository, dispatcher));
        }

        public void Dispose()
        {
            _runner.Dispose();
        }

        [Theory]
        [InlineData(987654321)]
        [InlineData(1000)]
        [InlineData(99.99)]
        [InlineData(0)]
        public async Task CanSetDailyWireTransfertLimit(decimal limit)
        {
            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var cmd = new SetDailyWireTransferLimit()
            {
                AccountId = _accountId,
                DailyLimit = limit
            };

            var limitSetEv = new DailyWireTransferLimitSet(CorrelatedMessage.NewRoot())
            {
                AccountId = cmd.AccountId,
                DailyLimit = cmd.DailyLimit
            };

            await _runner.Run(
                def => def.Given(accCreatedEv).When(cmd).Then(limitSetEv)
            );
        }

        [Theory]
        [InlineData(-987654321)]
        [InlineData(-1000)]
        [InlineData(-99.99)]
        [InlineData(-0.001)]

        public async Task CannotSetDailyWireTransfertLimit(decimal limit)
        {
            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var cmd = new SetDailyWireTransferLimit()
            {
                AccountId = _accountId,
                DailyLimit = limit
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv)
                    .When(cmd)
                    .Throws(new SystemException("The Daily Wire Transfer limit cannot be negative."))
            );
        }

        [Fact]
        public async Task CannotSetDailyWireTransferLimitIfAccountDoesNotExist()
        {
            var cmd = new SetDailyWireTransferLimit()
            {
                AccountId = _accountId,
                DailyLimit = 1000
            };

            await _runner.Run(
                def => def
                    .Given()
                    .When(cmd)
                    .Throws(new SystemException("The Daily Wire Transfer limit cannot be set on inexistent account."))
            );
        }
    }
}