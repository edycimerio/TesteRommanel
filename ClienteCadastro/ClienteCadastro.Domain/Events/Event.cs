using System;

namespace ClienteCadastro.Domain.Events
{
    public abstract class Event : IEvent
    {
        public Guid Id { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string EventType => GetType().Name;

        protected Event()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.Now;
        }
    }
}
