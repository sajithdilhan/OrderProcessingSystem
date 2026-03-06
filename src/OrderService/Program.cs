using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Middleware;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Persistent;
using OrderService.Infrastructure.Repositories;
using Scalar.AspNetCore;
using Shared.Contracts.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService.Application.Services.OrderService>();
var databaseName = builder.Configuration.GetConnectionString("OrderDatabase") ?? "OrderDatabase";
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseInMemoryDatabase(databaseName));

var rabbitSettings = builder.Configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>();
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitSettings!.Host,
                 rabbitSettings.VirtualHost,
                 h =>
                 {
                     h.Username(rabbitSettings.Username);
                     h.Password(rabbitSettings.Password);
                 });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Orders API")
               .WithTheme(ScalarTheme.DeepSpace)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
               .EnableDarkMode();
    });
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
