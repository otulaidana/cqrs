using System;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;
using System.Reactive.Disposables;
using CqrsAccount.Domain.Aggregates;
using NodaTime;

namespace CqrsAccount.Domain.Commands
{
    public class AccountCommandHandler
        : IHandleCommand<CreateAccount>
            , IHandleCommand<SetOverdraftLimit>
            , IHandleCommand<SetDailyWireTransferLimit>
            , IHandleCommand<DepositCheque>
            , IHandleCommand<DepositCash>
            , IHandleCommand<WithdrawCash>
            , IHandleCommand<TryWireTransfer>
            , IHandleCommand<StartNewBusinessDay>
            , IDisposable
    {
        readonly IRepository _repo;
        readonly IDisposable _disposer;

        public AccountCommandHandler(IRepository repo, ICommandSubscriber bus, IClock clock = null)
        {
            _repo = repo;

            _disposer = new CompositeDisposable(
                bus.Subscribe<CreateAccount>(this),
                bus.Subscribe<SetOverdraftLimit>(this),
                bus.Subscribe<SetDailyWireTransferLimit>(this),
                bus.Subscribe<DepositCheque>(this),
                bus.Subscribe<DepositCash>(this),
                bus.Subscribe<WithdrawCash>(this),
                bus.Subscribe<TryWireTransfer>(this),
                bus.Subscribe<StartNewBusinessDay>(this));
        }

        public void Dispose() => _disposer.Dispose();

        public CommandResponse Handle(CreateAccount command)
        {
            try
            {
                if (_repo.TryGetById<Account>(command.AccountId, out var res))
                {
                    throw new SystemException("The account with specified ID already exists.");
                }

                var account = Account.Create(command.AccountId, command.Name, command);

                _repo.Save(account);
                return command.Succeed();
            }
            catch (Exception e)
            {
                return command.Fail(e);
            }
        }

        public CommandResponse Handle(SetOverdraftLimit command)
        {
            try
            {
                if (!_repo.TryGetById<Account>(command.AccountId, out var account))
                {
                    throw new SystemException("The overdraft limit cannot be set on inexistent account.");
                }

                account.SetOverdraftLimit(command.OverdraftLimit, command);

                _repo.Save(account);
                return command.Succeed();
            }
            catch (Exception e)
            {
                return command.Fail(e);
            }
        }

        public CommandResponse Handle(SetDailyWireTransferLimit command)
        {
            try
            {
                if (!_repo.TryGetById<Account>(command.AccountId, out var account))
                {
                    throw new SystemException("The Daily Wire Transfer limit cannot be set on inexistent account.");
                }

                account.SetDailyWireTransferLimit(command.DailyLimit, command);

                _repo.Save(account);
                return command.Succeed();
            }
            catch (Exception e)
            {
                return command.Fail(e);
            }
        }

        public CommandResponse Handle(DepositCheque command)
        {
            try
            {
                if (!_repo.TryGetById<Account>(command.AccountId, out var account))
                {
                    throw new SystemException("The cheque cannot be deposited to the inexistent account.");
                }

                account.DepositCheque(command.Amount, command);

                _repo.Save(account);
                return command.Succeed();
            }
            catch (Exception e)
            {
                return command.Fail(e);
            }
        }

        public CommandResponse Handle(DepositCash command)
        {
            try
            {
                if (!_repo.TryGetById<Account>(command.AccountId, out var account))
                {
                    throw new SystemException("The cash cannot be deposited to the inexistent account.");
                }

                account.DepositCash(command.Amount, command);

                _repo.Save(account);
                return command.Succeed();
            }
            catch (Exception e)
            {
                return command.Fail(e);
            }
        }

        public CommandResponse Handle(WithdrawCash command)
        {
            try
            {
                if (!_repo.TryGetById<Account>(command.AccountId, out var account))
                {
                    throw new SystemException("Cash cannot be withdrawn from the inexistent accont.");
                }

                account.WithdrawCash(command.Amount, command);
                _repo.Save(account);

                return command.Succeed();
            }
            catch (Exception e)
            {
                return command.Fail(e);
            }
        }

        public CommandResponse Handle(TryWireTransfer command)
        {
            try
            {
                if (!_repo.TryGetById<Account>(command.AccountId, out var account))
                {
                    throw new SystemException("The wire transfer cannot be done on the inexistent account.");
                }

                account.TryWireTransfer(command.Amount, command);
                _repo.Save(account);

                return command.Succeed();
            }
            catch (Exception e)
            {
                return command.Fail(e);
            }
        }
    

        public CommandResponse Handle(StartNewBusinessDay command)
            {
                try
                {
                    if (!_repo.TryGetById<Account>(command.AccountId, out var account))
                    {
                        throw new SystemException("Invalid account.");
                    }

                    account.StartNewBusinessDay(command);
                    _repo.Save(account);

                    return command.Succeed();
                }
                catch (Exception e)
                {
                    return command.Fail(e);
                }
            }
    }
}
