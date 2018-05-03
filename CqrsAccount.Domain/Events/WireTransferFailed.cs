using System;
using ReactiveDomain.Messaging;
using Newtonsoft.Json;

namespace CqrsAccount.Domain.Events
{
    public class WireTransferFailed: Event
    {
        public WireTransferFailed(CorrelatedMessage source)
            : base(source)
        {
        }

        [JsonConstructor]
        public WireTransferFailed(CorrelationId correlationId, SourceId sourceId)
            : base(correlationId, sourceId)
        {
        }

        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }       

    }
}
