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
    public class SetOverdraftLimitTests : IDisposable
    {
        readonly Guid _accountId;
        readonly EventStoreScenarioRunner<Account> _runner;

        public SetOverdraftLimitTests(EventStoreFixture fixture)
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
        public async Task CanSetOverdraftLimit(decimal limit)
        {
            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var cmd = new SetOverdraftLimit
            {
                AccountId = _accountId,
                OverdraftLimit = limit
            };

            var limitSetEv = new OverdraftLimitSet(CorrelatedMessage.NewRoot())
            {
                AccountId = cmd.AccountId,
                OverdraftLimit = cmd.OverdraftLimit
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

        public async Task CannotSetOverdraftLimit(decimal limit)
        {
            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var cmd = new SetOverdraftLimit
            {
                AccountId = _accountId,
                OverdraftLimit = limit
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv)
                    .When(cmd)
                    .Throws(new SystemException("The overdraft limit cannot be negative."))
            );
        }

        [Fact]
        public async Task CannotSetOverdraftLimitIfAccountDoesNotExist()
        {
            var cmd = new SetOverdraftLimit
            {
                AccountId = _accountId,
                OverdraftLimit = 1000
            };

            await _runner.Run(
                def => def
                    .Given()
                    .When(cmd)
                    .Throws(new SystemException("The overdraft limit cannot be set on inexistent account."))
            );
        }
    }
}