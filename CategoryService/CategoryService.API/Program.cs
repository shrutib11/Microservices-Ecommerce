using System.Security.Claims;
using System.Text;
using CategoryService.API.GrpServices;
using CategoryService.Application.Interfaces;
using CategoryService.Application.Mappings;
using CategoryService.Application.Validators;
using CategoryService.Domain.Interfaces;
using CategoryService.Infrastructure;
using CategoryService.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using HashidsNet;
using Microservices.Shared;
using Microservices.Shared.Protos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var conn = builder.Configuration.GetConnectionString("CategoryServiceDbConnection");
builder.Services.AddDbContext<CategoryServiceDbContext>(options => options.UseNpgsql(conn));

builder.Services.AddSingleton<IHashids>(_ => new Hashids("mysecretsalt12345", 8));  
builder.Services.AddAutoMapper(typeof(CategoryProfile));
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService.Application.Services.CategoryService>();
builder.Services.AddFluentValidationAutoValidation(options =>
{
    options.DisableDataAnnotationsValidation = true;
});
builder.Services.AddValidatorsFromAssemblyContaining<CategoryDtoValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddGrpc();

builder.Services.AddGrpcClient<Product.ProductClient>(o =>
{
    o.Address = new Uri("https://localhost:5004");
});

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 4001;
});

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


var app = builder.Build();

app.UseHttpsRedirection();
app.UseMiddleware<ErrorHandlingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<CategoryGrpcService>();

app.Run();
