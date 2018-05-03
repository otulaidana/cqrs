using System;
using ReactiveDomain.Messaging;
using Newtonsoft.Json;

namespace CqrsAccount.Domain.Events
{
    using NodaTime;

    public class BusinessDayStarted : Event
    {
        public BusinessDayStarted(CorrelatedMessage source)
            : base(source)
        { }

        [JsonConstructor]
        public BusinessDayStarted(CorrelationId correlationId, SourceId sourceId)
            : base(correlationId, sourceId)
        { }

        public Guid AccountId { get; set; }

    }
}
