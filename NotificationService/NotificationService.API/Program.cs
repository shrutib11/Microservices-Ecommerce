using System.Security.Claims;
using System.Text;
using Microservices.Shared;
using Microservices.Shared.Interfaces;
using Microservices.Shared.Protos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotificationService.API.GrpcServices;
using NotificationService.API.Hubs;
using NotificationService.API.Services;
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
builder.Services.AddScoped<INotificationService, NotificationService.Application.Services.NotificationService>();
builder.Services.AddAutoMapper(typeof(NotificationProfile));
builder.Services.AddAutoMapper(typeof(UserNotificationProfile));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGrpcClient<User.UserClient>(o =>
{
    o.Address = new Uri("https://localhost:5006");
});

builder.Services.AddGrpc();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

var jwtSection = builder.Configuration.GetSection("Jwt");
var authority = jwtSection["Authority"];
var requireHttps = bool.Parse(jwtSection["RequireHttpsMetadata"] ?? "false");
var audiences = jwtSection.GetSection("Audiences").Get<string[]>();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.RequireHttpsMetadata = requireHttps;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority,

            ValidateAudience = true,
            ValidAudiences = audiences,

            RoleClaimType = ClaimTypes.Role,
            NameClaimType = "preferred_username"
        };
    });


builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: **Bearer {your JWT token}**"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
{
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
});
});

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddScoped<INotification, NotificationHubService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<NotificationGrpcService>();
app.MapHub<NotificationHub>("/hubs/notification");
app.Run();

