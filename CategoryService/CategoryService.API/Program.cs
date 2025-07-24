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
using Microservices.Shared.Middlewares;
using Microservices.Shared.Protos;
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
builder.Services.AddSwaggerGen();

builder.Services.AddGrpcClient<Product.ProductClient>(o =>
{
    o.Address = new Uri("https://localhost:5004");
});

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 4001;
});

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
// builder.Services.AddMemoryCache();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings!.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = "role"
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
        Description = "Enter 'Bearer' followed by your JWT token.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR..."
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
            Array.Empty<string>()
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
// app.UseMiddleware<JtiValidatorMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<CategoryGrpcService>();

app.Run();
