using System;
using ReactiveDomain.Messaging;
using Newtonsoft.Json;

namespace CqrsAccount.Domain.Events
{
    using System.Security.Principal;

    public class WireTransferHappened : Event
    {
        public WireTransferHappened(CorrelatedMessage source)
            : base(source)
        {
        }

        [JsonConstructor]
        public WireTransferHappened(CorrelationId correlationId, SourceId sourceId)
            : base(correlationId, sourceId)
        {
        }

        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }

    }
}
