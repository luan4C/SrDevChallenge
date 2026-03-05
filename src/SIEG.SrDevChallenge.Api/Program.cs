using SIEG.SrDevChallenge.Api.Configurations;
using SIEG.SrDevChallenge.Infrastructure.IoC;
using SIEG.SrDevChallenge.Application.IoC;
using SIEG.SrDevChallenge.Api.Endpoints;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load(@"..\..\.env");      
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.ConfigureMiddlewareServices();
builder.Services.ConfigureEnvironment(builder.Configuration);
builder.Services.ConfigureXMLServices();

// Só configura persistência se não for ambiente de teste
if (builder.Environment.EnvironmentName != "Testing")
{
    builder.Services.ConfigurePersistence(builder.Configuration);
    builder.Services.ConfigureRabbitMQ(builder.Configuration);
}

builder.Services.ConfigureApplicationServices();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.ConfigureMiddlewares();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapDocumentosEndpoints();

// Só configura MongoDB se não for ambiente de teste
if (app.Environment.EnvironmentName != "Testing")
{
    app.ConfigureMongoStartup();
}

app.Run();
