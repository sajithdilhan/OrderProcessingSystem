using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Events;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Repositories;
using Scalar.AspNetCore;
using Shared.Contracts.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService.Application.Services.NotificationService>();
var databaseName = builder.Configuration.GetConnectionString("NotificationDatabase") ?? "NotificationDatabase";
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseInMemoryDatabase(databaseName));

var rabbitSettings = builder.Configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentSucceededEventConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitSettings!.Host,
                 rabbitSettings.VirtualHost,
                 h =>
                 {
                     h.Username(rabbitSettings.Username);
                     h.Password(rabbitSettings.Password);
                 });
        cfg.ReceiveEndpoint("notification-service.payment-created", e => e.ConfigureConsumer<PaymentSucceededEventConsumer>(context));
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Notifications API")
               .WithTheme(ScalarTheme.DeepSpace)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
               .EnableDarkMode();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
