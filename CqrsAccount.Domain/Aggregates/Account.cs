using System;
using System.Collections.Generic;
using System.Text;
using ReactiveDomain;
using ReactiveDomain.Messaging;

namespace CqrsAccount.Domain.Aggregates
{
    using System.Security.Policy;
    using CqrsAccount.Domain.Commands;
    using CqrsAccount.Domain.Events;
    using NodaTime;


    public class Account : EventDrivenStateMachine
    {        
        decimal _overdraftLimit;
        decimal _dailyTransferLimit;
        decimal _dailyLimitUsed;
        decimal _pendingAmount;
        decimal _currentBalance;
        decimal _currentDayTotalTransfer;
        bool _blocked;

        public Account()
        {            
            _overdraftLimit = -1;
            _dailyTransferLimit = -1;
            _dailyLimitUsed = 0;
            _pendingAmount = 0;
            _currentBalance = 0;
            _currentDayTotalTransfer = 0;
            _blocked = false;

            Register<AccountCreated>(e => { Id = e.AccountId; });
            Register<AccountBlocked>(e => { _blocked = true; });
            Register<AccountUnblocked>(e => { _blocked = false; });
            Register<CashDeposited>(e => { _currentBalance = _currentBalance + e.Amount; });
            Register<CashWithdrawn>(
                e =>
                {
                    _currentBalance = _currentBalance - e.Amount;
                    _dailyLimitUsed = _dailyLimitUsed + e.Amount;
                });
            Register<WithdrawalFailed>(e => { _blocked = true; });
            Register<ChequeDeposited>(
                e =>
                {
                    Id = e.AccountId;
                    _pendingAmount = _pendingAmount + e.Amount;
                });
            Register<DailyWireTransferLimitSet>(e => { _dailyTransferLimit = e.DailyLimit; });
            Register<OverdraftLimitSet>(e => { _overdraftLimit = e.OverdraftLimit; });
            Register<WireTransferFailed>(e => { _blocked = true;});
            Register<WireTransferHappened>(
                e =>
                {
                    _currentDayTotalTransfer = _currentDayTotalTransfer + e.Amount;
                    _currentBalance = _currentBalance - e.Amount;
                    _dailyLimitUsed = _dailyLimitUsed + e.Amount;
                });
            Register<BusinessDayStarted>(
                e =>
                {
                    _currentBalance = _currentBalance + _pendingAmount;
                    _pendingAmount = 0;
                    _dailyLimitUsed = 0;
                });
        }

        public static Account Create(
            Guid accountId,
            string name,
            CorrelatedMessage source)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new SystemException("The account name is invalid.");

            var account = new Account();
            account.Raise(
                new AccountCreated(source)
                {
                    AccountId = accountId,
                    Name = name
                });
            return account;
        }

        public void SetOverdraftLimit(
            decimal overdraftLimit,
            CorrelatedMessage source)
        {
            if (overdraftLimit < 0)
                throw new SystemException("The overdraft limit cannot be negative.");

            Raise(
                new OverdraftLimitSet(source)
                {
                    AccountId = Id,
                    OverdraftLimit = overdraftLimit
                });
        }

        public void SetDailyWireTransferLimit(
            decimal dailyLimit,
            CorrelatedMessage source)
        {
            if (dailyLimit < 0)
                throw new SystemException("The Daily Wire Transfer limit cannot be negative.");

            Raise(
                new DailyWireTransferLimitSet(source)
                {
                    AccountId = Id,
                    DailyLimit = dailyLimit
                });
        }

        public void DepositCheque(
            decimal amount,
            CorrelatedMessage source)
        {
            if (amount <= 0)
            {
                throw new SystemException("The deposited cheque amount should be greater than 0.");
            }

            Raise(
                new ChequeDeposited(source)
                {
                    AccountId = Id,
                    Amount = amount
                });
            
        }

        public void DepositCash(
            decimal amount,
            CorrelatedMessage source)
        {
            if (amount <= 0)
            {
                throw new SystemException("The deposited cash amount should be greater than 0.");
            }

            Raise(
                new CashDeposited(source)
                {
                    AccountId = Id,
                    Amount = amount
                });

            if (_blocked)
            {
                Raise(
                    new AccountUnblocked(source)
                    {
                        AccountId = Id
                    });
            }
        }

        public void WithdrawCash(decimal amount, CorrelatedMessage source)
        {
            if (amount <= 0)
            {
                throw new SystemException("Withdrawn Cash amount should be greater than 0.");
            }

            if (_overdraftLimit == -1)
            {
                if (amount > _currentBalance)
                {
                    throw new SystemException("The account does not have enough funds for requested wire transfer.");
                }
            }
            else
            {
                if (amount > _currentBalance + _overdraftLimit)
                {
                    Raise(
                        new WithdrawalFailed(source)
                        {
                            AccountId = Id,
                            Amount = amount
                        }
                    );

                    Raise(
                        new AccountBlocked(source)
                        {
                            AccountId = Id
                        }
                    );

                    return;
                }
            }


            if (_blocked)
            {
                Raise(
                    new WithdrawalFailed(source)
                    {
                        AccountId = Id,
                        Amount = amount
                    }
                );
                return;
            }

            Raise(
                new CashWithdrawn(source)
                {
                    AccountId = Id,
                    Amount = amount
                }
            );

        }

        public void TryWireTransfer(decimal amount, CorrelatedMessage source)
        {
            if (amount <= 0)
            {
                throw new SystemException("Wire Transfer amount should be greater than 0.");
            }

            if (amount > _currentBalance)
            {
                throw new SystemException("The account does not have enough funds for requested wire transfer.");
            }

            if (_dailyTransferLimit != -1 && _dailyTransferLimit -_dailyLimitUsed < amount )
            {
                Raise(
                    new WireTransferFailed(source)
                    {
                        AccountId = Id,
                        Amount = amount
                    }
                );

                Raise(
                    new AccountBlocked(source)
                    {
                        AccountId = Id
                    }
                );
                return;
            }

            if (_blocked)
            {
                Raise(
                    new WireTransferFailed(source)
                    {
                        AccountId = Id,
                        Amount = amount
                    }
                );
            }
            else
            {
                Raise(
                    new WireTransferHappened(source)
                    {
                        AccountId = Id,
                        Amount = amount
                    }
                );
            }
        }

        public void StartNewBusinessDay(CorrelatedMessage source)
            {
                if (_blocked && _pendingAmount > 0)
                {
                    Raise(
                        new AccountUnblocked(source)
                        {
                            AccountId = Id
                        }
                    );
                }
            }
    }
}
