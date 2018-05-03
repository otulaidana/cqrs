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
    public class DepositChequeTests : IDisposable
    {
        readonly Guid _accountId;
        readonly EventStoreScenarioRunner<Account> _runner;

        public DepositChequeTests(EventStoreFixture fixture)
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
        [InlineData(0.001)]
        public async Task CanDepositCheque(decimal amount)
        {
            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var cmd = new DepositCheque()
            {
                AccountId = _accountId,
                Amount = amount
            };

            var amountSetEv = new ChequeDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = cmd.AccountId,
                Amount = cmd.Amount
            };

            await _runner.Run(
                def => def.Given(accCreatedEv).When(cmd).Then(amountSetEv)
            );
        }

        [Theory]
        [InlineData(-987654321)]
        [InlineData(-1000)]
        [InlineData(-99.99)]
        [InlineData(0)]

        public async Task CannotDepositCheque(decimal amount)
        {
            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var cmd = new DepositCheque()
            {
                AccountId = _accountId,
                Amount = amount
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv)
                    .When(cmd)
                    .Throws(new SystemException("The deposited cheque amount should be greater than 0."))
            );
        }

        [Fact]
        public async Task CannotDepositChequeIfAccountDoesNotExist()
        {
            var cmd = new DepositCheque()
            {
                AccountId = _accountId,
                Amount = 1000
            };

            await _runner.Run(
                def => def
                    .Given()
                    .When(cmd)
                    .Throws(new SystemException("The cheque cannot be deposited to the inexistent account."))
            );
        }

        [Fact]
        public async Task CannotUnblockAccountAfterDepositCheque()
        {
            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var accBlockedEv = new AccountBlocked(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId
            };

            var cmd = new DepositCheque()
            {
                AccountId = _accountId,
                Amount = 1000
            };

            var amountSetEv = new ChequeDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = cmd.AccountId,
                Amount = cmd.Amount
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv, accBlockedEv)
                    .When(cmd)
                    .Then(amountSetEv));
        }

        [Fact]
        public async Task CanUnblockAccountOnNextBusinessDay()
        {
            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var accBlockedEv = new AccountBlocked(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId
            };

            var depositedEv = new ChequeDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = 1000m
            };

            var cmd = new StartNewBusinessDay()
            {
                AccountId = _accountId
            };

            var accUnblockedEv = new AccountUnblocked(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv, accBlockedEv, depositedEv)
                    .When(cmd)
                    .Then(accUnblockedEv));
        }
    }
}