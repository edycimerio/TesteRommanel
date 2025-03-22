using System;

namespace ClienteCadastro.Domain.Events
{
    public interface IEvent
    {
        Guid Id { get; }
        DateTime Timestamp { get; }
        string EventType { get; }
    }
}
