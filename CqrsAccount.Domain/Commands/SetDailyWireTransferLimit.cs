using System;
using ReactiveDomain.Messaging;

namespace CqrsAccount.Domain.Commands
{
    public class SetDailyWireTransferLimit: Command
    {
        public SetDailyWireTransferLimit()
            : base(NewRoot())
        {
        }
        public Guid AccountId { get; set; }
        public decimal DailyLimit { get; set; }
    }
}
