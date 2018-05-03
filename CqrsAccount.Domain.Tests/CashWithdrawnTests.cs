using System;
using System.Threading.Tasks;
using CqrsAccount.Domain.Tests.Common;
using CqrsAccount.Domain.Aggregates;
using CqrsAccount.Domain.Commands;
using CqrsAccount.Domain.Events;
using EventStore.Core.Exceptions;
using ReactiveDomain.Messaging;
using Xunit;
using Xunit.ScenarioReporting;

namespace CqrsAccount.Domain.Tests
{    
    [Collection("AggregateTest")]
    public class CashWithdrawnTests:IDisposable
    {
        readonly Guid _accountId;
        readonly EventStoreScenarioRunner<Account> _runner;

        public CashWithdrawnTests(EventStoreFixture fixture)
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
        [InlineData(987654321, 987654321)]
        [InlineData(2000, 1000)]
        [InlineData(199, 99.99)]
        [InlineData(5.25, 0.0001)]
        public async Task CanWithdrawCash(decimal deposit, decimal withdraw)
        {
            
            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var depositedEv = new CashDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = deposit
            };

            var cmd = new WithdrawCash()
            {
                AccountId = _accountId,
                Amount = withdraw
            };

            var withdrawnEv = new CashWithdrawn(cmd)
            {
                AccountId = _accountId,
                Amount = cmd.Amount
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv, depositedEv)
                    .When(cmd)
                    .Then(withdrawnEv));

        }

        [Theory]
        [InlineData(0)]
        [InlineData(-0.001)]
        [InlineData(-1000)]
        [InlineData(-987654321)]
        public async Task CannotWithrawNonPoitiveCashTests(decimal amount)
        {
            var accCreateEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var cmd = new WithdrawCash()
            {
                AccountId = _accountId,
                Amount = amount
            };

            await _runner.Run(
                def => def
                    .Given(accCreateEv)
                    .When(cmd)
                    .Throws(new SystemException("Withdrawn Cash amount should be greater than 0.")));

        }

        [Fact]
        public async Task CannotWithdrawCashIfAccountDoesNotExist()
        {
            var cmd = new WithdrawCash()
            {
                AccountId = _accountId,
                Amount = 1000
            };

            await _runner.Run(
                def => def
                    .Given()
                    .When(cmd)
                    .Throws(new SystemException("Cash cannot be withdrawn from the inexistent accont.")));
        }

        [Fact]
        public async Task CannotWithdrawCashIfAccountIsBlocked()
        {
            var deposit = 5000m;
            var withdraw = 1000m;

            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var depositSetEv = new CashDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = deposit
            };

            var accBlockedEv = new AccountBlocked(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId
            };

            var cmd = new WithdrawCash()
            {
                AccountId = _accountId,
                Amount = withdraw
            };

            var failedEv = new WithdrawalFailed(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = cmd.Amount
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv, depositSetEv, accBlockedEv)
                    .When(cmd)
                    .Then(failedEv));
        }

        [Theory]
        [InlineData(1000, 3000, 3500)]
        [InlineData(3000, 1000, 3500)]
        [InlineData(99.99, 55.55, 77.77)]
        [InlineData(300, 200, 500)]
        [InlineData(300, 0.05, 300.02)]
        public async Task CanWithdrawCashFromOverdraft(decimal deposit, decimal overdraft, decimal withdraw)
        {

            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var depositSetEv = new CashDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = deposit
            };

            var overdraftSetEv = new OverdraftLimitSet(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                OverdraftLimit = overdraft
            };

            var cmd = new WithdrawCash()
            {
                AccountId = _accountId,
                Amount = withdraw
            };

            var withdrawnEv = new CashWithdrawn(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = cmd.Amount
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv, depositSetEv, overdraftSetEv)
                    .When(cmd)
                    .Then(withdrawnEv));
        }

        [Fact]
        public async Task CannotWithdrawCashIfNotEnoughFunds()
        {
            var deposit = 500m;
            var withdraw = 1000m;

            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var depositSetEv = new CashDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = deposit
            };
           
            var cmd = new WithdrawCash()
            {
                AccountId = _accountId,
                Amount = withdraw
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv, depositSetEv)
                    .When(cmd)
                    .Throws(new SystemException("The account does not have enough funds for requested wire transfer.")));
        }

        [Theory]
        [InlineData(1000, 3000, 5500)]
        [InlineData(3000, 1000, 5500)]
        [InlineData(99.99, 55.55, 177.77)]
        [InlineData(300, 200, 505)]
        [InlineData(300, 0.05, 300.06)]
        public async Task CannnotWithdrawCashIfOverdrafdIsExceeded(decimal deposit, decimal overdraft, decimal withdraw)
        {

            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var depositSetEv = new CashDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = deposit
            };

            var overdraftSetEv = new OverdraftLimitSet(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                OverdraftLimit = overdraft
            };

            var cmd = new WithdrawCash()
            {
                AccountId = _accountId,
                Amount = withdraw
            };

            var withdrawalFailedEv = new WithdrawalFailed(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = cmd.Amount
            };

            var accBlockedEv = new AccountBlocked(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId
            };

            await _runner.Run(
                    def => def
                        .Given(accCreatedEv, depositSetEv, overdraftSetEv)
                        .When(cmd)
                        .Then(withdrawalFailedEv, accBlockedEv));
        }

        [Fact]
        public async Task CannnotWithdrawCashFromBlockedAccount()
        {
            var deposit = 1000m;
            var withdraw = 500m;

            var accCreatedEv = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var depositSetEv = new CashDeposited(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = deposit
            };

            var accBlockedEv = new AccountBlocked(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId
            };

            var cmd = new WithdrawCash()
            {
                AccountId = _accountId,
                Amount = withdraw
            };

            var transferFailedEv = new WithdrawalFailed(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Amount = cmd.Amount
            };

            await _runner.Run(
                def => def
                    .Given(accCreatedEv, depositSetEv, accBlockedEv)
                    .When(cmd)
                    .Then(transferFailedEv));
        }

    }
}
