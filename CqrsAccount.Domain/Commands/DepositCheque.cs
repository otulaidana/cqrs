using System;
using ReactiveDomain.Messaging;

namespace CqrsAccount.Domain.Commands
{
    public class DepositCheque: Command
    {
        public DepositCheque()
            : base(NewRoot())
        {
        }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
    }
}
