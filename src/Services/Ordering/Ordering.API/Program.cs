using EventBus.Messages.Common;
using MassTransit;
using Ordering.API.EventBusConsumer;
using System.Reflection;
using Ordering.Application.CrossCutting;
using Ordering.Infrastructure.CrossCutting;
using Ordering.API.Extensions;
using Ordering.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Project Services
var configuration = builder.Configuration;
var server = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? string.Empty;
var user = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? string.Empty;
var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? string.Empty;
Console.WriteLine($"CAIO---{server};{user};{password}");

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(configuration);

//MassTransit configuration
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(server, "/", h =>
        {
            h.Username(user);
            h.Password(password);
        });

        cfg.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, c => 
        {
            c.ConfigureConsumer<BasketCheckoutConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });

    x.AddConsumer<BasketCheckoutConsumer>();
});

//
builder.Services.AddScoped<BasketCheckoutConsumer>();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//Add migration
app.MigrateDatabase<OrderContext>((context, services) =>
{
    var logger = services.GetService<ILogger<OrderContextSeed>>();
    OrderContextSeed
        .SeedAsync(context, logger)
        .Wait();
});

app.Run();
