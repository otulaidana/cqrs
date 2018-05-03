using System;
using ReactiveDomain.Messaging;
using Newtonsoft.Json;


namespace CqrsAccount.Domain.Events
{
    public class AccountCreated : Event
    {
        public AccountCreated(CorrelatedMessage source)
            : base(source)
        { }

        [JsonConstructor]
        public AccountCreated(CorrelationId correlationId, SourceId sourceId)
            : base(correlationId, sourceId)
        { }

        public Guid AccountId { get; set; }

        public string Name { get; set; }

    }
}
