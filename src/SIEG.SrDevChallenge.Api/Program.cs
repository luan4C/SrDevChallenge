using SIEG.SrDevChallenge.Api.Configurations;
using SIEG.SrDevChallenge.Infrastructure.IoC;
using SIEG.SrDevChallenge.Application.IoC;
using SIEG.SrDevChallenge.Api.Endpoints;
using MongoDB.Driver;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load(@"..\..\.env");      
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.ConfigureSwagger();
builder.Services.ConfigureMiddlewareServices();
builder.Services.ConfigureEnvironment(builder.Configuration);
builder.Services.ConfigureXMLServices();
builder.Services.AddRateLimiter(e=>
{    
    e.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });
});

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
app.UseSwaggerDocumentation();

app.UseHttpsRedirection();

app.MapDocumentosEndpoints();


// Só configura MongoDB se não for ambiente de teste
if (app.Environment.EnvironmentName != "Testing")
{
    app.ConfigureMongoStartup();
}
app.UseRateLimiter();
app.Run();
