using System;
using ReactiveDomain.Messaging;

namespace CqrsAccount.Domain.Commands
{
    public class WithdrawCash:Command
    {
        public WithdrawCash()
            : base(NewRoot())
        {
        }

        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }

    }
}
