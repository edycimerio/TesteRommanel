using System;

namespace ClienteCadastro.Domain.Events
{
    public class StoredEvent
    {
        public Guid Id { get; private set; }
        public Guid AggregateId { get; private set; }
        public string AggregateType { get; private set; }
        public string EventType { get; private set; }
        public string Data { get; private set; }
        public DateTime Timestamp { get; private set; }
        public int Version { get; private set; }

        public StoredEvent(Guid id, Guid aggregateId, string aggregateType, string eventType, string data, DateTime timestamp, int version)
        {
            Id = id;
            AggregateId = aggregateId;
            AggregateType = aggregateType;
            EventType = eventType;
            Data = data;
            Timestamp = timestamp;
            Version = version;
        }
    }
}
