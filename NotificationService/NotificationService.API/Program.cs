using Microservices.Shared;
using Microservices.Shared.Protos;
using Microsoft.EntityFrameworkCore;
using NotificationService.API.GrpcServices;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Mappings;
using NotificationService.Domain.Interfaces;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var conn = builder.Configuration.GetConnectionString("NotificationServiceConnection");
builder.Services.AddDbContext<NotificationServiceDbContext>(options =>options.UseNpgsql(conn!));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddScoped<INotificationRepository, NotificationsRepository>();
builder.Services.AddScoped<IUserNotificationRepository, UserNotificationsRepository>();
builder.Services.AddScoped<INotificationService, NotificationService.Application.Services.NotificationService>();
builder.Services.AddAutoMapper(typeof(NotificationProfile));
builder.Services.AddAutoMapper(typeof(UserNotificationProfile));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpcClient<User.UserClient>(o =>
{
    o.Address = new Uri("https://localhost:5006");
});
builder.Services.AddGrpc();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<NotificationGrpcService>();

app.Run();
