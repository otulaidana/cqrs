using System;
using ReactiveDomain.Messaging;



namespace CqrsAccount.Domain.Events
{
    using Newtonsoft.Json;

    public class ChequeDeposited : Event
    {
        public ChequeDeposited(CorrelatedMessage source)
            : base(source)
        { }

        [JsonConstructor]
        public ChequeDeposited(CorrelationId correlationId, SourceId sourceId)
            : base(correlationId, sourceId)
        { }

        public Guid AccountId { get; set; }

        public decimal Amount { get; set; }

    }
}
