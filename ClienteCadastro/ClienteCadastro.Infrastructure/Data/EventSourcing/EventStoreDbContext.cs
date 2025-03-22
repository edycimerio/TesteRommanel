using ClienteCadastro.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace ClienteCadastro.Infrastructure.Data.EventSourcing
{
    public class EventStoreDbContext : DbContext
    {
        public DbSet<StoredEvent> StoredEvents { get; set; }

        public EventStoreDbContext(DbContextOptions<EventStoreDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StoredEvent>(entity =>
            {
                entity.ToTable("EventStore");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.AggregateId)
                    .IsRequired();
                
                entity.Property(e => e.AggregateType)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(100)");
                
                entity.Property(e => e.EventType)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(100)");
                
                entity.Property(e => e.Data)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(MAX)");
                
                entity.Property(e => e.Timestamp)
                    .IsRequired()
                    .HasColumnType("DATETIME");
                
                entity.Property(e => e.Version)
                    .IsRequired()
                    .HasColumnType("INT");
                
                entity.HasIndex(e => new { e.AggregateId, e.Version }).IsUnique();
            });
        }
    }
}
