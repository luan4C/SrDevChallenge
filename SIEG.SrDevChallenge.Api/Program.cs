using SIEG.SrDevChallenge.Api.Configurations;
using SIEG.SrDevChallenge.Infrastructure.IoC;
using SIEG.SrDevChallenge.Application.IoC;
using SIEG.SrDevChallenge.Api.Endpoints;
using MongoDB.Driver;

DotNetEnv.Env.Load();      
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.ConfigureEnvironment();
builder.Services.ConfigureXMLServices();
builder.Services.ConfigurePersistence(builder.Configuration);
builder.Services.ConfigureApplicationServices();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//TODO: mover para outra configuração
app.MapDocumentosEndpoints();

app.UseHttpsRedirection();


app.Run();
