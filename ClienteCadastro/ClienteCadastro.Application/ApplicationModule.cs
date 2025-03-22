using System.Reflection;
using ClienteCadastro.Application.Behaviors;
using ClienteCadastro.Application.Commands.Cliente.Insert;
using ClienteCadastro.Application.Commands.Cliente.Insert.Validators;
using ClienteCadastro.Application.Commands.Cliente.Update;
using ClienteCadastro.Application.Commands.Cliente.Update.Validators;
using ClienteCadastro.Application.Interfaces;
using ClienteCadastro.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ClienteCadastro.Application
{
    public static class ApplicationModule
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Registrar AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Registrar MediatR
            services.AddMediatR(cfg => 
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                
                // Adicionar behaviors
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            });

            // Registrar validadores manualmente
            services.AddScoped<IValidator<CreateClienteCommand>, CreateClienteCommandValidator>();
            services.AddScoped<IValidator<UpdateClienteCommand>, UpdateClienteCommandValidator>();

            // Registrar serviços
            services.AddScoped<IClienteService, ClienteService>();

            return services;
        }
    }
}
