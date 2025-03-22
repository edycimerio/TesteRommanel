using ClienteCadastro.Application;
using ClienteCadastro.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adiciona a infraestrutura (repositórios, contextos, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Adiciona a camada de aplicação (CQRS, validação, etc.)
builder.Services.AddApplication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
