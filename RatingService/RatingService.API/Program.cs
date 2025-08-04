using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microservices.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RatingService.Application.Interfaces;
using RatingService.Application.Mappings;
using RatingService.Application.Validators;
using RatingService.Domain.Interfcaes;
using RatingService.Infrastructure;
using RatingService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
var conn = builder.Configuration.GetConnectionString("RatingServiceConnection");
builder.Services.AddDbContext<RatingServiceDbContext>(options =>options.UseNpgsql(conn!));

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation(options =>
{
    options.DisableDataAnnotationsValidation = true;
});
builder.Services.AddValidatorsFromAssemblyContaining<RatingDtoValidator>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IRatingService, RatingService.Application.Services.RatingService>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddAutoMapper(typeof(RatingProfile));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
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

builder.Services.AddGrpcClient<Microservices.Shared.Protos.Product.ProductClient>(o =>
{
    o.Address = new Uri("https://localhost:5004");
});

builder.Services.AddGrpcClient<Microservices.Shared.Protos.User.UserClient>(o =>
{
    o.Address = new Uri("https://localhost:5006"); 
});


builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 4007;
});

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

app.Run();
