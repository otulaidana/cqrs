using System;
using ReactiveDomain.Messaging;

namespace CqrsAccount.Domain.Commands
{    
    public class TryWireTransfer:Command
    {
        public TryWireTransfer()
            : base(NewRoot())
        {
        }

        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }

    }
}
