using Basket.API.Mappers;
using Basket.API.Repositories;
using Basket.API.Repositories.Interfaces;
using MassTransit;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Basket.API", Version = "v1" });
});

//MassTrasint configuration
var configuration = builder.Configuration;
var server = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? string.Empty;
var user = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? string.Empty;
var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? string.Empty;

builder.Services.AddMassTransit((x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(server, "/", h =>
        {
            h.Username(user);
            h.Password(password);
        });

        cfg.ConfigureEndpoints(context);
    });
}));

// General configuration
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddAutoMapper(typeof(BasketProfile));

// Redis cnfiguration
var redisServer = Environment.GetEnvironmentVariable("DB_HOST") ?? string.Empty;
var redisPort = Environment.GetEnvironmentVariable("DB_PORT") ?? string.Empty;
var redisConnectionString = $"{redisServer}:{redisPort}";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
