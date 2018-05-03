using System;
using ReactiveDomain.Messaging;

namespace CqrsAccount.Domain.Commands
{
    public class CreateAccount : Command
    {
        public CreateAccount()
            : base(NewRoot())
        {
        }

        public Guid AccountId { get; set; }

        public string Name { get; set; }

        public decimal? Balance { get; set; }
    }
}
