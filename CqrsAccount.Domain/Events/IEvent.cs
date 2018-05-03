using System;
using System.Collections.Generic;
using System.Text;

namespace CqrsAccount.Domain.Events
{
    public interface IEvent
    {
        Guid Id { get; }

        string EventType { get; }

        byte[] Data { get; }

        byte[] MetaData { get; }

    }
}
