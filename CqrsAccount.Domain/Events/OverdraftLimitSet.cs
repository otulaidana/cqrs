using System;
using ReactiveDomain.Messaging;



namespace CqrsAccount.Domain.Events
{
    using Newtonsoft.Json;

    public class OverdraftLimitSet : Event
    {
        public OverdraftLimitSet(CorrelatedMessage source)
            : base(source)
        { }

        [JsonConstructor]
        public OverdraftLimitSet(CorrelationId correlationId, SourceId sourceId)
            : base(correlationId, sourceId)
        { }

        public Guid AccountId { get; set; }

        public decimal OverdraftLimit { get; set; }

    }
}
