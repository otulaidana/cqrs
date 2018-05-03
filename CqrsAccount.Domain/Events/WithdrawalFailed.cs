using System;
using ReactiveDomain.Messaging;
using Newtonsoft.Json;

namespace CqrsAccount.Domain.Events
{
    public class WithdrawalFailed : Event
    {
        public WithdrawalFailed(CorrelatedMessage source)
            : base(source)
        {
        }

        [JsonConstructor]
        public WithdrawalFailed(CorrelationId correlationId, SourceId sourceId)
            : base(correlationId, sourceId)
        {
        }

        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }       

    }
}
