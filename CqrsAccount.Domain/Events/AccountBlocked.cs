using System;
using ReactiveDomain.Messaging;
using Newtonsoft.Json;

namespace CqrsAccount.Domain.Events
{
    using NodaTime;

    public class AccountBlocked : Event
    {
        public AccountBlocked(CorrelatedMessage source)
            : base(source)
        { }

        [JsonConstructor]
        public AccountBlocked(CorrelationId correlationId, SourceId sourceId)
            : base(correlationId, sourceId)
        { }

        public Guid AccountId { get; set; }

        public bool Blocked { get; set; }       
    }
}
