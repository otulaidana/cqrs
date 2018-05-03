using System;
using ReactiveDomain.Messaging;
using Newtonsoft.Json;


namespace CqrsAccount.Domain.Events
{


    public class CashWithdrawn:Event
    {
        public CashWithdrawn(CorrelatedMessage source)
            : base(source)
        {
        }

        [JsonConstructor]
        protected CashWithdrawn(CorrelationId correlationId, SourceId sourceId)
            : base(correlationId, sourceId)
        {
        }

        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }

    }
}
