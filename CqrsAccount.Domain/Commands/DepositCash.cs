using System;
using ReactiveDomain.Messaging;

namespace CqrsAccount.Domain.Commands
{
    public class DepositCash: Command
    {
        public DepositCash()
            : base(NewRoot())
        {
        }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
    }
}
