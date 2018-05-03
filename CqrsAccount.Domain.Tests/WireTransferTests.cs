using System;
using System.Threading.Tasks;
using CqrsAccount.Domain.Tests.Common;
using CqrsAccount.Domain.Aggregates;
using CqrsAccount.Domain.Commands;
using CqrsAccount.Domain.Events;
using ReactiveDomain.Messaging;
using Xunit;
using Xunit.ScenarioReporting;
using NodaTime;

namespace CqrsAccount.Domain.Tests
{
    [Collection("AggregateTest")]
    public class WireTransferTests: IDisposable
    {
        readonly Guid _accountId;
        readonly EventStoreScenarioRunner<Account> _runner;
        readonly SystemClock _clock;
        public WireTransferTests(EventStoreFixture fixture)
        {
            _accountId = Guid.NewGuid();
            _clock = SystemClock.Instance;
            _runner = new EventStoreScenarioRunner<Account>(
                _accountId,
                fixture,
                (repository, dispatcher) => new AccountCommandHandler(repository, dispatcher, _clock));
        }
        public void Dispose()
        {
            _runner.Dispose();
        }

        [Theory]
        [InlineData(1000, 500, 100)]
        [InlineData(1000, 500, 500)]
        [InlineData(20000000, 20000000, 10000000)]
        [InlineData(66.66, 55.55, 44.44)]
        public async Task CanWireTransferIfEnoughFundsAndNotExceededLimits(
            decimal deposit, decimal limit, decimal transfer)
        {
            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var cashDepositedEv = new CashDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = deposit
            };

            var dailyLimitSetEv = new DailyWireTransferLimitSet(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                DailyLimit = limit
            };

            var cmd = new TryWireTransfer()
            {
                AccountId = _accountId,
                Amount = transfer
            };

            var transferedEv = new WireTransferHappened(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = cmd.Amount
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv, cashDepositedEv, dailyLimitSetEv)
                    .When(cmd)
                    .Then(transferedEv));
        }

        [Theory]
        [InlineData(1000, 5000)]
        [InlineData(1000, 1000.01)]
        [InlineData(66.66, 77.77)]
        public async Task CannotWireTransferIfNotEnoughFunds(
            decimal deposit, decimal transfer)
        {
            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var cashDepositedEv = new CashDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = deposit
            };

            var cmd = new TryWireTransfer()
            {
                AccountId = _accountId,
                Amount = transfer
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv, cashDepositedEv)
                    .When(cmd)
                    .Throws(new SystemException("The account does not have enough funds for requested wire transfer.")));

        }

        [Theory]
        [InlineData(1000, 500, 600)]
        [InlineData(1000, 500, 500.01)]
        [InlineData(77.77, 55.55, 66.66)]

        public async Task CannotWireTranferIfExceedsLimit(decimal deposit, decimal limit, decimal transfer)
        {
            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var cashDepositedEv = new CashDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = deposit
            };

            var limitSetEv = new DailyWireTransferLimitSet(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                DailyLimit = limit
            };

            var cmd = new TryWireTransfer()
            {
                AccountId = _accountId,
                Amount = transfer
            };

            var transferFailedEv = new WireTransferFailed(cmd)
            {
                AccountId = _accountId,
                Amount = cmd.Amount
            };

            var accBlockedEv = new AccountBlocked(cmd)
            {
                AccountId = _accountId
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv, cashDepositedEv, limitSetEv)
                    .When(cmd)
                    .Then(transferFailedEv, accBlockedEv));
        }

        [Theory]
        [InlineData(-1000)]
        [InlineData(0)]
        [InlineData(-1.01)]
        [InlineData(-100000000)]
        public async Task CannotWithdrawFundsIfTransferIsNotPositive(decimal transfer)
        {
            var deposit = 5000m;
            var limit = 5000m;

            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var cashDepositedEv = new CashDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = deposit
            };

            var limitSetEv = new DailyWireTransferLimitSet(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                DailyLimit = limit
            };

            var cmd = new TryWireTransfer()
            {
                AccountId = _accountId,
                Amount = transfer
            };
  
            await _runner.Run(
                def => def
                    .Given(accCreatedEv, cashDepositedEv, limitSetEv)
                    .When(cmd)
                    .Throws(new SystemException("Wire Transfer amount should be greater than 0.")));
        }

        [Fact]
        public async Task CannotWireTransferIfAccountDoesNotExist()
        {
            var transfer = 500m;

            var cmd = new TryWireTransfer()
            {
                AccountId = _accountId,
                Amount = transfer
            };

            await _runner.Run(
                def => def
                    .Given()
                    .When(cmd)
                    .Throws(new SystemException("The wire transfer cannot be done on the inexistent account.")));
        }

        [Fact]
        public async Task CannotWireTransferIfAccountIsBlocked()
        {
            var deposit = 5000m;
            var transfer = 100m;

            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var cashDepositedEv = new CashDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = deposit
            };

            var accBlocked = new AccountBlocked(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId
            };

            var cmd = new TryWireTransfer()
            {
                AccountId = _accountId,
                Amount = transfer
            };

            var transferFailedEv = new WireTransferFailed(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = cmd.Amount
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv, cashDepositedEv, accBlocked)
                    .When(cmd)
                    .Then(transferFailedEv));
        }

        [Theory]
        [InlineData(1000, 500, 100)]
        [InlineData(1000, 300, 100)]
        [InlineData(50000000, 50000000, 10000000)]
        [InlineData(99.99, 55.55, 11.11)]
        public async Task CanWireTransferMultipleTimesIfLimitNotExceeded(
            decimal deposit, decimal limit, decimal transfer)
        {
            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var cashDepositedEv = new CashDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = deposit
            };

            var dailyLimitSetEv = new DailyWireTransferLimitSet(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                DailyLimit = limit
            };

            var cmd = new TryWireTransfer()
            {
                AccountId = _accountId,
                Amount = transfer
            };

            var transferedEv = new WireTransferHappened(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = transfer
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv, cashDepositedEv, dailyLimitSetEv, transferedEv, transferedEv)
                    .When(cmd)
                    .Then(transferedEv));
        }

        [Theory]
        [InlineData(1000, 500, 200)]
        [InlineData(1000, 300, 150)]
        [InlineData(80000000, 50000000, 20000000)]
        [InlineData(99.99, 55.55, 22.22)]
        public async Task CannotWireTransferMultipleTimesIfLimitExceeded(
            decimal deposit, decimal limit, decimal transfer)
        {
            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var cashDepositedEv = new CashDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = deposit
            };

            var dailyLimitSetEv = new DailyWireTransferLimitSet(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                DailyLimit = limit
            };

            var transferedEv = new WireTransferHappened(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = transfer
            };

            var cmd = new TryWireTransfer()
            {
                AccountId = _accountId,
                Amount = transfer
            };

            var transferFailedEv = new WireTransferFailed(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = transfer
            };

            var accBlockedEv = new AccountBlocked(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv, cashDepositedEv, dailyLimitSetEv, transferedEv, transferedEv)
                    .When(cmd)
                    .Then(transferFailedEv, accBlockedEv));
        }

    }
}
