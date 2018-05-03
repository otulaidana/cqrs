using System;
using ReactiveDomain.Messaging;

namespace CqrsAccount.Domain.Events
{
    using Newtonsoft.Json;

    public class DailyWireTransferLimitSet : Event
    {
        public DailyWireTransferLimitSet(CorrelatedMessage source)
            : base(source)
        { }

        [JsonConstructor]
        public DailyWireTransferLimitSet(CorrelationId correlationId, SourceId sourceId)
            : base(correlationId, sourceId)
        { }

        public Guid AccountId { get; set; }

        public decimal DailyLimit { get; set; }

    }
}
