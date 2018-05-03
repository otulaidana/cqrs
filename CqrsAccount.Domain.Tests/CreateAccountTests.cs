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
    public class CreateAccountTests : IDisposable
    {
        readonly Guid _accountId;
        readonly EventStoreScenarioRunner<Account> _runner;

        public CreateAccountTests(EventStoreFixture fixture)
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

        [Fact]
        public async Task CanCreateAccount()
        {
            var cmd = new CreateAccount
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            var ev = new AccountCreated(cmd)
            {
                AccountId = cmd.AccountId,
                Name = cmd.Name
            };

            await _runner.Run(
                def => def.Given().When(cmd).Then(ev)
            );
        }

        [Fact]
        public async Task CannotCreateAccountWithSameId()
        {
            var cmd = new CreateAccount()
            {
                AccountId = _accountId,
                Name = "J S"
            };

            var ev = new AccountCreated(CorrelatedMessage.NewRoot())
            {
                AccountId = _accountId,
                Name = "Jake Sanders"
            };

            await _runner.Run(
                def => def.Given(ev).When(cmd).Throws(new SystemException("The account with specified ID already exists."))
            );
        }

        [Fact]
        public async Task CannotCreateAccountWithEmptyName()
        {
            var cmd = new CreateAccount()
            {
                AccountId = _accountId,
                Name = String.Empty
            };

            await _runner.Run(
                def => def
                    .Given()
                    .When(cmd)
                    .Throws( new SystemException("The account name is invalid."))
            );
        }

        [Fact]
        public async Task CannotCreateAccountWithNullName()
        {
            var cmd = new CreateAccount()
            {
                AccountId = _accountId,
                Name = null
            };

            await _runner.Run(
                def => def
                    .Given()
                    .When(cmd)
                    .Throws(new SystemException("The account name is invalid."))
            );
        }

        [Fact]
        public async Task CannotCreateAccountWithSpaceName()
        {
            var cmd = new CreateAccount()
            {
                AccountId = _accountId,
                Name = "\t"
            };

            await _runner.Run(
                def => def
                    .Given()
                    .When(cmd)
                    .Throws(new SystemException("The account name is invalid."))
            );
        }
    }
}