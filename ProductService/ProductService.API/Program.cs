using System.Security.Claims;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using HashidsNet;
using Microservices.Shared;
using Microservices.Shared.Protos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductService.API.GrpcServices;
using ProductService.Application.Interfaces;
using ProductService.Application.Validators;
using ProductService.Domain.Interfaces;
using ProductService.Infrastructure;
using ProductService.Infrastructure.Repositories;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);
var conn = builder.Configuration.GetConnectionString("ProductDb");

builder.Services.AddSingleton<IHashids>(_ => new Hashids("mysecretsalt12345", 8));
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(conn!));

builder.Services.AddControllers()
.AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddFluentValidationAutoValidation(options =>
{
    options.DisableDataAnnotationsValidation = true;
});

builder.Services.AddValidatorsFromAssemblyContaining<ProductDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ProductMediaDtoValidator>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductMediaRepository, ProductMediaRepository>();
builder.Services.AddScoped<IProductService, ProductService.Application.Services.ProductService>();
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGrpc();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 104857600;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});


builder.Services.AddGrpcClient<Category.CategoryClient>(o =>
{
    o.Address = new Uri("https://localhost:5003");
});

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 4002;
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.MapGrpcService<ProductGrpcService>();

app.Run();

// var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
// builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// builder.Services.AddAuthentication("Bearer")
//     .AddJwtBearer("Bearer", options =>
//     {
//         options.TokenValidationParameters = new()
//         {
//             ValidateIssuer = false,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             ValidAudience = jwtSettings?.Audience,
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.Key)),
//         };
//     });

// builder.Services.AddSwaggerGen(c =>
// {
//     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Name = "Authorization",
//         Type = SecuritySchemeType.Http,
//         Scheme = "Bearer",
//         BearerFormat = "JWT",
//         In = ParameterLocation.Header,
//         Description = "Enter 'Bearer' followed by your JWT token.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR..."
//     });

//     //tells Swagger that all endpoints will require this "Bearer" security scheme 
//     c.AddSecurityRequirement(new OpenApiSecurityRequirement
//     {
//         {
//             new OpenApiSecurityScheme
//             {
//                 Reference = new OpenApiReference
//                 {
//                     Type = ReferenceType.SecurityScheme,
//                     Id = "Bearer"
//                 }
//             },
//             Array.Empty<string>()
//         }
//     });
// });