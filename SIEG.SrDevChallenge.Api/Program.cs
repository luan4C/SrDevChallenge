using SIEG.SrDevChallenge.Api.Configurations;
using SIEG.SrDevChallenge.Infrastructure.IoC;
using SIEG.SrDevChallenge.Application.IoC;
using SIEG.SrDevChallenge.Api.Endpoints;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load(@"..\.env");      
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.ConfigureMiddlewareServices();
builder.ConfigureEnvironment();
builder.Services.ConfigureXMLServices();
builder.Services.ConfigurePersistence(builder.Configuration);
builder.Services.ConfigureApplicationServices();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.ConfigureMiddlewares();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//TODO: mover para outra configuração
app.MapDocumentosEndpoints();

app.ConfigureMongoStartup();

app.Run();
