using System.Reflection;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using HashidsNet;
using Microservices.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UserService.API.GrpcServices;
using UserService.API.Validators;
using UserService.Application.Interfaces;
using UserService.Application.Mappings;
using UserService.Application.Services;
using UserService.Domain.Interfaces;
using UserService.Infrastructure;
using UserService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddEndpointsApiExplorer();
var conn = builder.Configuration.GetConnectionString("UserServiceConnection");
builder.Services.AddDbContext<UserServiceDbContext>(options => options.UseNpgsql(conn));

builder.Services.AddSingleton<IHashids>(_ => new Hashids("mysecretsalt12345", 8));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService.Application.Services.UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IKeyclockService, KeyclockService>();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly(), typeof(UserProfile).Assembly);

var configuration = builder.Configuration;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = configuration["Keycloack:Authority"];
    options.RequireHttpsMetadata = bool.Parse(configuration["Keycloack:RequireHttpsMetadata"] ?? "false");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = configuration["Keycloack:Authority"],
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = "preferred_username"
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: **Bearer {your JWT token}**"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthorization();

builder.Services.AddFluentValidationAutoValidation(options =>
{
    options.DisableDataAnnotationsValidation = true;
});

builder.Services.AddValidatorsFromAssemblyContaining<UserDtoValidator>();
builder.Services.AddGrpc();
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 4000;
});

builder.Services.AddHttpClient();


var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI(c =>
//     {
//         c.SwaggerEndpoint("/swagger/v1/swagger.json", "User MicroService");
//     });
// }

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User MicroService");
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();
app.MapGrpcService<UserGrpcService>();

app.Run();