using System;
using System.Threading;
using System.Threading.Tasks;
using ClienteCadastro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClienteCadastro.Infrastructure.Data
{
    public class ClienteDbContext : DbContext
    {
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Endereco> Enderecos { get; set; } = null!;

        public ClienteDbContext(DbContextOptions<ClienteDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Clientes");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.TipoPessoa)
                    .IsRequired()
                    .HasColumnType("CHAR(1)");
                
                entity.Property(e => e.Nome)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(100)");
                
                entity.Property(e => e.Documento)
                    .IsRequired()
                    .HasColumnType("VARCHAR(18)");
                
                entity.Property(e => e.IE)
                    .HasColumnType("VARCHAR(20)");
                
                entity.Property(e => e.IsIsentoIE)
                    .HasColumnType("BIT");
                
                entity.Property(e => e.DataNascimento)
                    .HasColumnType("DATE");
                
                entity.Property(e => e.Telefone)
                    .HasColumnType("VARCHAR(20)");
                
                entity.Property(e => e.Email)
                    .HasColumnType("VARCHAR(100)");
                
                entity.Property(e => e.DataCriacao)
                    .IsRequired()
                    .HasColumnType("DATETIME");
                
                entity.Property(e => e.DataAtualizacao)
                    .HasColumnType("DATETIME");
                
                entity.Property(e => e.Ativo)
                    .IsRequired()
                    .HasColumnType("BIT");
                
                entity.HasIndex(e => e.Documento).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Endereço
            modelBuilder.Entity<Endereco>(entity =>
            {
                entity.ToTable("Enderecos");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.CEP)
                    .IsRequired()
                    .HasColumnType("VARCHAR(9)");
                
                entity.Property(e => e.Logradouro)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(100)");
                
                entity.Property(e => e.Numero)
                    .IsRequired()
                    .HasColumnType("VARCHAR(20)");
                
                entity.Property(e => e.Complemento)
                    .HasColumnType("NVARCHAR(50)");
                
                entity.Property(e => e.Bairro)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(50)");
                
                entity.Property(e => e.Cidade)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(50)");
                
                entity.Property(e => e.Estado)
                    .IsRequired()
                    .HasColumnType("CHAR(2)");
                
                entity.Property(e => e.DataCriacao)
                    .IsRequired()
                    .HasColumnType("DATETIME");
                
                entity.Property(e => e.DataAtualizacao)
                    .HasColumnType("DATETIME");
                
                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.Enderecos)
                    .HasForeignKey(e => e.ClienteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is Entity entity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        // Não precisamos definir DataCriacao aqui, pois já é definido no construtor da entidade
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entity.AtualizarDataModificacao();
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
