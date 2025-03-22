using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ClienteCadastro.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace ClienteCadastro.Infrastructure.Data.EventSourcing
{
    public class EventStore : IEventStore
    {
        private readonly EventStoreDbContext _context;

        public EventStore(EventStoreDbContext context)
        {
            _context = context;
        }

        public async Task SaveEventAsync<T>(T @event, Guid aggregateId, string aggregateType, int version) where T : IEvent
        {
            var serializedData = JsonSerializer.Serialize(@event);
            
            var storedEvent = new StoredEvent(
                @event.Id,
                aggregateId,
                aggregateType,
                @event.EventType,
                serializedData,
                @event.Timestamp,
                version
            );

            await _context.StoredEvents.AddAsync(storedEvent);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<StoredEvent>> GetEventsByAggregateIdAsync(Guid aggregateId)
        {
            return await _context.StoredEvents
                .Where(e => e.AggregateId == aggregateId)
                .OrderBy(e => e.Version)
                .ToListAsync();
        }

        public async Task<IEnumerable<StoredEvent>> GetAllEventsAsync(int pageSize, int pageNumber)
        {
            return await _context.StoredEvents
                .OrderByDescending(e => e.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
