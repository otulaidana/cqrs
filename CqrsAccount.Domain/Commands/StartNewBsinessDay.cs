using System;
using ReactiveDomain.Messaging;

namespace CqrsAccount.Domain.Commands
{
    public class StartNewBusinessDay: Command
    {
        public StartNewBusinessDay()
            : base(NewRoot())
        {
        }
        public Guid AccountId { get; set; }        
    }
}
