using ClienteCadastro.Domain.Events;
using ClienteCadastro.Domain.Interfaces;
using ClienteCadastro.Infrastructure.Data;
using ClienteCadastro.Infrastructure.Data.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClienteCadastro.Infrastructure
{
    public static class InfrastructureModule
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuração dos contextos
            services.AddDbContext<ClienteDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<EventStoreDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Repositórios
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IEnderecoRepository, EnderecoRepository>();
            
            // UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Event Sourcing
            services.AddScoped<IEventStore, EventStore>();

            return services;
        }
    }
}
