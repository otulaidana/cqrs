using System;
using ReactiveDomain.Messaging;

namespace CqrsAccount.Domain.Commands
{
    public class SetOverdraftLimit: Command
    {
        public SetOverdraftLimit()
            : base(NewRoot())
        {
        }
        public Guid AccountId { get; set; }
        public decimal OverdraftLimit { get; set; }
    }
}
