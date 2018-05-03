using System;
using ReactiveDomain.Messaging;



namespace CqrsAccount.Domain.Events
{
    using Newtonsoft.Json;

    public class CashDeposited : Event
    {
        public CashDeposited(CorrelatedMessage source)
            : base(source)
        { }

        [JsonConstructor]
        public CashDeposited(CorrelationId correlationId, SourceId sourceId)
            : base(correlationId, sourceId)
        { }

        public Guid AccountId { get; set; }

        public decimal Amount { get; set; }

    }
}
