using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClienteCadastro.Domain.Events
{
    public interface IEventStore
    {
        Task SaveEventAsync<T>(T @event, Guid aggregateId, string aggregateType, int version) where T : IEvent;
        Task<IEnumerable<StoredEvent>> GetEventsByAggregateIdAsync(Guid aggregateId);
        Task<IEnumerable<StoredEvent>> GetAllEventsAsync(int pageSize, int pageNumber);
    }
}
